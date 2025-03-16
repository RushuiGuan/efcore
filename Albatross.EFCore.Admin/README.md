# About

A library that can be used to create efcore admin commandline utility.

# Quick Start

The library contains the following generic utilities:

1. [ExecuteMigrationScripts](./ExecuteDeploymentScripts.cs) - A command that will execute sql scripts. It takes on a
   dependency on interface [IExecuteScriptFile](./IExecuteScriptFile.cs). The interface should be registered with one of
   the two implementations [ExecuteScriptFile](./ExecuteScriptFile.cs)
   or [ExecuteSqlServerScriptFile](./ExecuteSqlServerScriptFile.cs).
	* `--directory` is a required option to specify the input script directory. The utility will only execute scripts
	  within a subfolder of the directory whose name matches the version of the entry assembly. For example, if the
	  input directory is `c:\deployment` and the entry assembly version is `2.0.0`, the target folder would be
	  `c:\deployment\2.0.0`. The prerelease part of the version will not be considered as part of the
	  folder name. Version `2.0.0-prerelease` and `2.0.0` will have the same target folder of `2.0.0`.
	* If the `--pre-migration` flag is set, the command should be executed prior the actual migration. The utility
	  however will skip execution if the target database has no pending migrations.
2. [EFMigrate](./EFMigrate.cs) - A command that will execute the efcore migration
3. [GenerateSqlScript](./GenerateSqlScript.cs) - A command that will generate the sql script for a specific db context.

# Putting it all together

Use the following sample projects for reference

* [Sample.Models](../Sample.Models)
* [Sample.Admin](../Sample.Admin)
* [Sample.SqlServer](../Sample.SqlServer)
* [Sample.Postgres](../Sample.Postgres)

Notice that the `Sample.Admin` project leverages the utilities of the `Albatross.EFCore.Admin` project by
creating [verbs](../Sample.Admin/Verbs.cs) that reference the admin project. The `Sample.Admin` only needs to create its
own migration db context such as [SampleSqlMigration](../Sample.Admin/SampleSqlMigration.cs)
and [SamplePostgresMigration](../Sample.Admin/SamplePostgresMigration.cs).

Since most projects only target either sql server or postgres, the code in `Sample.Models` and `Sample.SqlServer` or
`Sample.Postgres` could be combined into a single project. But an admin project such as `Sample.Admin` should always be
used to seperate the admin utility from the actual program itself.  `Sample.Admin` contains its own dbcontext
class [SamplePostgresMigration](../Sample.Admin/SamplePostgresMigration.cs) that derives from the actual db context
class [SampleDbSession](../Sample.Models/SampleDbSession.cs) because migration dbcontext has different requirements.

1. At the root of the admin project, generate the efcore migration using the dotnet efcore utility

```
dotnet ef migrations add SampleSqlServerMigration_$name --context SampleSqlServerMigration --output-dir (Get-Path Migrations, SqlServer)
```
2. Run the migration by calling the ef-migrate command of the `Sample.Admin` project

```
Sample.Admin.exe sql ef-migrate --verbose Information
```
3. Run the script generation command to see and verify the actual table setup.
```
Sample.Admin.exe sql create-sql-script;
```