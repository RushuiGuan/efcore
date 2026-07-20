using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Albatross.EFCore.CodeAnalysis {
	/// <summary>
	/// Warns when a database transaction created within a method (a local of type
	/// <c>Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction</c>) is never committed. A transaction is
	/// considered committed when the same method either calls <c>CommitAsync</c> on it, or passes it to a
	/// <c>SaveAndCommitAsync</c> call.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MissingTransactionCommitAnalyzer : DiagnosticAnalyzer {
		public const string DiagnosticId = "ALBEFCORE001";

		const string transactionTypeName = "IDbContextTransaction";
		const string transactionNamespace = "Microsoft.EntityFrameworkCore.Storage";
		const string commitMethodName = "Commit";
		const string commitAsyncMethodName = "CommitAsync";
		const string saveAndCommitMethodName = "SaveAndCommitAsync";

		static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: "Transaction is missing a commit",
			messageFormat: "Transaction '{0}' is created but never committed. Commit it (Commit/CommitAsync) or pass it to SaveAndCommitAsync.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "A database transaction created by BeginTransactionAsync should be committed with CommitAsync or SaveAndCommitAsync before the method returns; otherwise the transaction is rolled back and its work is lost.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

		public override void Initialize(AnalysisContext context) {
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCodeBlockAction(AnalyzeCodeBlock);
		}

		static void AnalyzeCodeBlock(CodeBlockAnalysisContext context) {
			var semanticModel = context.SemanticModel;
			foreach (var local in context.CodeBlock.DescendantNodes().OfType<LocalDeclarationStatementSyntax>()) {
				foreach (var declarator in local.Declaration.Variables) {
					var initializer = declarator.Initializer?.Value;
					// only consider transactions the method itself creates via a call
					if (initializer == null || !IsInvocation(initializer)) {
						continue;
					}
					if (!(semanticModel.GetDeclaredSymbol(declarator) is ILocalSymbol symbol) || !IsTransactionType(symbol.Type)) {
						continue;
					}
					if (!IsCommitted(context.CodeBlock, symbol, semanticModel)) {
						context.ReportDiagnostic(Diagnostic.Create(rule, declarator.Identifier.GetLocation(), symbol.Name));
					}
				}
			}
		}

		static bool IsInvocation(ExpressionSyntax expression) {
			if (expression is AwaitExpressionSyntax await) {
				expression = await.Expression;
			}
			return expression is InvocationExpressionSyntax;
		}

		static bool IsTransactionType(ITypeSymbol? type) {
			if (type == null) {
				return false;
			}
			return IsNamedTransaction(type) || type.AllInterfaces.Any(IsNamedTransaction);
		}

		static bool IsNamedTransaction(ITypeSymbol type) =>
			type.Name == transactionTypeName && type.ContainingNamespace?.ToDisplayString() == transactionNamespace;

		static bool IsCommitted(SyntaxNode codeBlock, ILocalSymbol transaction, SemanticModel semanticModel) {
			foreach (var invocation in codeBlock.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
				// transaction.Commit(...) / transaction.CommitAsync(...)
				if (invocation.Expression is MemberAccessExpressionSyntax member
						&& (member.Name.Identifier.Text == commitMethodName || member.Name.Identifier.Text == commitAsyncMethodName)
						&& IsReferenceTo(member.Expression, transaction, semanticModel)) {
					return true;
				}
				// SaveAndCommitAsync(transaction, ...) — the transaction is passed as an argument
				if (GetInvokedMethodName(invocation.Expression) == saveAndCommitMethodName
						&& invocation.ArgumentList.Arguments.Any(arg => IsReferenceTo(arg.Expression, transaction, semanticModel))) {
					return true;
				}
			}
			return false;
		}

		static string? GetInvokedMethodName(ExpressionSyntax expression) {
			switch (expression) {
				case MemberAccessExpressionSyntax member:
					return member.Name.Identifier.Text;
				case IdentifierNameSyntax identifier:
					return identifier.Identifier.Text;
				default:
					return null;
			}
		}

		static bool IsReferenceTo(ExpressionSyntax expression, ILocalSymbol transaction, SemanticModel semanticModel) {
			var symbol = semanticModel.GetSymbolInfo(expression).Symbol;
			return symbol != null && SymbolEqualityComparer.Default.Equals(symbol, transaction);
		}
	}
}
