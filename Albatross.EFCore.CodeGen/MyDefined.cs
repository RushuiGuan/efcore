using Albatross.CodeGen;
using Albatross.CodeGen.CSharp.Expressions;
using System;

namespace Albatross.EFCore.CodeGen {
	public static class MyDefined {
		public static class Namespaces {
			public readonly static NamespaceExpression MicrosoftEntityFrameworkCore = new NamespaceExpression("Microsoft.EntityFrameworkCore");
		}
		public static class Identifiers {
			public readonly static IIdentifierNameExpression ModelBuilder = new QualifiedIdentifierNameExpression("ModelBuilder", Namespaces.MicrosoftEntityFrameworkCore);
			public readonly static IdentifierNameExpression ModelBuilderVariable = new IdentifierNameExpression("modelBuilder");
			public readonly static IdentifierNameExpression Build = new IdentifierNameExpression("Build");
		}
	}
}
