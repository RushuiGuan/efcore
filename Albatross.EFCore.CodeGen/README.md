# About

A companion code generator used by the [Albatross.EFCore](../Albatross.EFCore) library.

# Features
Instead of using reflection at runtime, this library will generate entity model builder at compile time using roslyn.

# Quick Start
* Create an entity
  ```csharp
  public class Address {
    public int Id { get; set; }
    public string? City { get; set; }
  }
  ```
* Crate the entity mapping class for the entity
  ```csharp
  public class AddressEntityMap : EntityMap<Address> {
    public override void Map(EntityTypeBuilder<Address> builder) {
        builder.HasKey(x => x.Id);
    }
  }
  ```
* The code generator will pick up on the `AddressEntityMap` class and generate the following code
  ```csharp
  public static ModelBuilder BuildEntityModels(this ModelBuilder modelBuilder)
  {
    new Sample.Models.AddressEntityMap().Build(modelBuilder);
    return modelBuilder;
  }
  ```
* To consume the generated code, use it directly in your custom `DbContext` class
  ```csharp
  public class SampleDbSession : DbSession {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.HasDefaultSchema(My.Schema.Sample);
      modelBuilder.BuildEntityModels();
    }
  }
  ```
* To turn on the debugging model and see the generated code in visual studio, add the following config change to the project file.
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <EmitAlbatrossCodeGenDebugFile>true</EmitAlbatrossCodeGenDebugFile>
    </PropertyGroup>
    <ItemGroup>
        <CompilerVisibleProperty Include="EmitAlbatrossCodeGenDebugFile" />
    </ItemGroup>
</Project>
```
* Rider shows the roslyn generated code at:  `Dependencies -> .NET 8.0 -> Source Generators`
* Sample code can be found here:
  * [Sample.EFCore](../Sample.EFCore)
  * [Sample.SqlServer](../Sample.SqlServer)
  * [Sample.Postgres](../Sample.Postgres)
  * [Sample.Admin](../Sample.Admin)