# About
A set of libraries help with setup of efcore for sql server and postgresql


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

# Libraries
| Name| Description||
|-|-|-|
| [Albatross.EFCore](./Albatross.EFCore)|The base library that help constructs dbcontext and entity mapping|[![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore)](https://www.nuget.org/packages/Albatross.EFCore)|
| [Albatross.EFCore.Admin](./Albatross.EFCore.Admin)| The base library that help constructs an efcore admin project| [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.Admin)](https://www.nuget.org/packages/Albatross.EFCore.Admin)                         |
| [Albatross.EFCore.Audit](./Albatross.EFCore.Audit)|| [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.Audit)](https://www.nuget.org/packages/Albatross.EFCore.Audit)|
| [Albatross.EFCore.AutoCacheEviction](./Albatross.EFCore.AutoCacheEviction) || [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.AutoCacheEviction)](https://www.nuget.org/packages/Albatross.EFCore.AutoCacheEviction) |
| [Albatross.EFCore.CodeGen](./Albatross.EFCore.CodeGen)| A dev dependency for `Albatross.EFCore`.  It uses rosyln code generator to create and register entity model builders | [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.CodeGen)](https://www.nuget.org/packages/Albatross.EFCore.CodeGen)                     |
| [Albatross.EFCore.PostgreSQL](./Albatross.EFCore.PostgreSQL)| Contains additional setup for postgresql| [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.PostgreSQL)](https://www.nuget.org/packages/Albatross.EFCore.PostgreSQL)               |
| [Albatross.EFCore.SqlServer](./Albatross.EFCore.SqlServer)| Contains additional setup for sql server| [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.SqlServer)](https://www.nuget.org/packages/Albatross.EFCore.SqlServer)                 |
| [Albatross.EFCore.ChangeReporting](./Albatross.EFCore.ChangeReporting)|| [![NuGet Version](https://img.shields.io/nuget/v/Albatross.EFCore.ChangeReporting)](https://www.nuget.org/packages/Albatross.EFCore.ChangeReporting)     |
| [Albatross.Testing.EFCore](./Albatross.Testing.EFCore)| A unit testing library that contains classes to create mock `DbSet<T>` and `IQueryable<T>` | [![NuGet Version](https://img.shields.io/nuget/v/Albatross.Testing.EFCore)](https://www.nuget.org/packages/Albatross.Testing.EFCore)                     |
