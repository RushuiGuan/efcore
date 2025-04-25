# About
An library that help setup efcore for sql server and postgres databases.

## Prerequisite
**Dotnet Compiler version 4.12.0 or higher**

`Albatross.EFCore.CodeGen` - an integrated part of the library that uses Roslyn for code generation takes on 
the dependency of `Microsoft.CodeAnalysis.CSharp` version 4.12.0, which in term requires the compiler version 4.12.0 or above.  The requirement is met with dotnet SDK version 9 or above or with the latest version of visual studio.  If dotnet SDK 8 is used with Console, VSCode or JetBrain Rider, the compiler version by default would be 4.11.0 or below.  It can be updated at the project level by referencing the `Microsoft.Net.Compilers.Toolset` version 4.12.0 or above in the project file.  The following code snippet shows how to do that.
```xml
<PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="4.12.0">
	<PrivateAssets>all</PrivateAssets>
	<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```