using Albatross.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Albatross.EFCore.CodeGen {
	[Generator]
	public class EntityModelBuilderClassCodeGenerator : IIncrementalGenerator {
		public void Initialize(IncrementalGeneratorInitializationContext context) {
			var compilationProvider = context.CompilationProvider;
			var classes = context.SyntaxProvider.CreateSyntaxProvider(
				static (node, _) => node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax or Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax,
				static (ctx, _) => {
					var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol;
					if (symbol != null && symbol.IsConcreteClass()) {
						return symbol;
					}
					return null;
				}
			).Where(static x => x != null);

			compilationProvider.Combine(classes.Collect()).Select(static (tuple, _) => { 
				var (compilation, classSymbols) = tuple;
			});
		}
		public void Execute(GeneratorExecutionContext context) {
			using var writer = new StringWriter();
			string? dbSessionNamespace = null;
			try {
				// System.Diagnostics.Debugger.Launch();
				var entityModelBuilderClasses = new List<INamedTypeSymbol>();

				foreach (var syntaxTree in context.Compilation.SyntaxTrees) {
					var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
					var walker = new EntityModelClassWalker(context.Compilation, semanticModel);
					walker.Visit(syntaxTree.GetRoot());
					entityModelBuilderClasses.AddRange(walker.EntityModelBuilderClasses);
					if (string.IsNullOrEmpty(dbSessionNamespace)) {
						dbSessionNamespace = walker.DbSessionClass?.ContainingNamespace.ToDisplayString();
					}
				}

				if (!entityModelBuilderClasses.Any()) {
					return;
				} else {
					// if the setup class is not found, use the namespace of the first option class
					if (string.IsNullOrEmpty(dbSessionNamespace)) {
						dbSessionNamespace = entityModelBuilderClasses.First().ContainingNamespace.ToDisplayString();
					}
				}

				var codeStack = new CodeStack();
				using (codeStack.NewScope(new CompilationUnitBuilder())) {
					codeStack.With(new UsingDirectiveNode("Albatross.EFCore"));
					codeStack.With(new UsingDirectiveNode(Shared.Namespace.System_Collections_Generic));
					codeStack.With(new UsingDirectiveNode(Shared.Namespace.Microsoft_EntityFrameworkCore));
					using (codeStack.NewScope(new NamespaceDeclarationBuilder(dbSessionNamespace ?? "DbSessionNamespaceNotYetFound"))) {
						using (codeStack.NewScope(new ClassDeclarationBuilder("CodeGen").Public().Static())) {
							using (codeStack.NewScope(new MethodDeclarationBuilder("ModelBuilder", "BuildEntityModels").Public().Static())) {
								codeStack.With(new ParameterNode(new TypeNode("ModelBuilder"), "modelBuilder").WithThis());
								foreach (var setup in entityModelBuilderClasses) {
									using (codeStack.NewScope()) {
										codeStack.Complete(new NewObjectBuilder(setup.GetFullName()))
											.With(new IdentifierNode("Build"))
											.ToNewBegin(new InvocationExpressionBuilder())
											.With(new ArgumentListBuilder().Build([new IdentifierNode("modelBuilder").Node]))
											.End();
									}
								}

								codeStack.With(SyntaxFactory.ReturnStatement(new IdentifierNode("modelBuilder").Identifier));
							}
						}
					}
				}

				var code = codeStack.Build();
				context.AddSource(Shared.Class.CodeGenExtensions, SourceText.From(code, Encoding.UTF8));
				writer.WriteSourceHeader(Shared.Class.CodeGenExtensions);
				writer.WriteLine(code);
			} catch (Exception err) {
				writer.WriteLine(err.ToString());
				context.CodeGenDiagnostic(DiagnosticSeverity.Error, $"{My.Diagnostic.IdPrefix}2", err.BuildCodeGeneneratorErrorMessage("commandline"));
			} finally {
				if(context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.EmitAlbatrossCodeGenDebugFile", out var value)) {
					if(bool.TryParse(value, out var emitDebugFile) && emitDebugFile) {
						var text = writer.ToString();
						context.CreateGeneratorDebugFile("albatross-efcore-codegen.debug.txt", text);
					}
				}
			}
		}
	}
}