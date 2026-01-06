using Albatross.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Albatross.EFCore.CodeGen {
	public static class MySymbolProvider {
		public static INamedTypeSymbol IBuildEntityModel(this Compilation compilation) => compilation.GetRequiredSymbol("Albatross.EFCore.IBuildEntityModel");
		public static INamedTypeSymbol DbSession(this Compilation compilation) => compilation.GetRequiredSymbol("Albatross.EFCore.DbSession");
	}
}
