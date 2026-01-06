using Albatross.CodeAnalysis;
using Albatross.CodeGen;
using Albatross.CodeGen.CSharp;
using Albatross.CodeGen.CSharp.Declarations;
using Albatross.CodeGen.CSharp.Expressions;
using Albatross.CodeGen.CSharp.TypeConversions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Albatross.EFCore.CodeGen {
	[Generator]
	public class EntityModelBuilderClassCodeGenerator : IIncrementalGenerator {
		public void Initialize(IncrementalGeneratorInitializationContext context) {
			var compilationProvider = context.CompilationProvider.Select(static (x, _) => x);
			var dbSessionClasses = context.SyntaxProvider.CreateSyntaxProvider(
				static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
				static (ctx, _) => {
					var compilation = ctx.SemanticModel.Compilation;
					var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol;
					if (symbol != null && symbol.IsDerivedFrom(compilation.DbSession())) {
						return symbol;
					}
					return null;
				}
			).Where(static x => x != null);

			var builderClasses = context.SyntaxProvider.CreateSyntaxProvider(
				static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
				static (ctx, _) => {
					var compilation = ctx.SemanticModel.Compilation;
					var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol;
					if (symbol != null && symbol.IsConcreteClass()
							&& symbol.AllInterfaces.Any(x => compilation.IBuildEntityModel().Is(x))
							&& (!symbol.Constructors.Any() || symbol.Constructors.Any(x => x.Parameters.Length == 0))) {
						return symbol;
					}
					return null;
				}
			).Where(static x => x != null);

			var aggregate = compilationProvider.Combine(dbSessionClasses.Collect()).Combine(builderClasses.Collect());
			context.RegisterSourceOutput(
				aggregate,
				static (context, data) => {
					var (tuple, builderClasses) = data;
					var (compilation, dbSessionClasses) = tuple;
					if (builderClasses.Any()) {
						var @namespace = dbSessionClasses.FirstOrDefault()?.ContainingNamespace.GetFullNamespace();
						var typeConverter = new DefaultTypeConverter();
						var file = new FileDeclaration("CodeGenExtensions") {
							Namespace = @namespace != null ? new NamespaceExpression(@namespace) : null,
							Classes = [
								new ClassDeclaration {
								IsStatic = true,
								Name = new IdentifierNameExpression("CodeGenExtensions"),
								AccessModifier = Defined.Keywords.Public,
								Methods = [
									new MethodDeclaration {
										Name = new IdentifierNameExpression("BuildEntityModels"),
										IsStatic = true,
										AccessModifier = Defined.Keywords.Public,
										ReturnType = new TypeExpression(MyDefined.Identifiers.ModelBuilder),
										Parameters = {
											new ParameterDeclaration{
												Name = MyDefined.Identifiers.ModelBuilderVariable,
												Type = new TypeExpression(MyDefined.Identifiers.ModelBuilder),
												UseThisKeyword = true,
											}
										},
										Body = {
											builderClasses.Select(x=> new NewObjectExpression{
												Type = typeConverter.Convert(x!),
											}.Chain(false, new InvocationExpression{
												CallableExpression = MyDefined.Identifiers.Build,
												Arguments = {
													MyDefined.Identifiers.ModelBuilderVariable,
												}
											})),
											new ReturnExpression{
												Expression = MyDefined.Identifiers.ModelBuilderVariable
											}
										}
									}
								]
							}
							],
						};
						context.AddSource(file.FileName, new StringWriter().Code(file).ToString());
					}
				}
			);
		}
	}
}