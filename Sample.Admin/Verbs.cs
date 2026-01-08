using Albatross.CommandLine.Annotations;
using Albatross.EFCore.Admin;
using Sample.Admin;

[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<SamplePostgresMigration>>("postgres exec-script", Description = "Execute postgres deployment scripts")]
[assembly: Verb<GenerateSqlScriptParams, GenerateSqlScript<SamplePostgresMigration>>("postgres create-script", Description = "Generate sql script for postgres")]
[assembly: Verb<EFMigrationParams, EFMigrate<SamplePostgresMigration>>("postgres ef-migrate", Description = "Migrate postgres database using dotnet ef tool")]


[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<SampleSqlServerMigration>>("sqlserver exec-script", Description = "Execute sql server deployment scripts")]
[assembly: Verb<GenerateSqlScriptParams, GenerateSqlScript<SampleSqlServerMigration>>("sqlserver create-script", Description = "Generate sql script for sql server")]
[assembly: Verb<EFMigrationParams, EFMigrate<SampleSqlServerMigration>>("sqlserver ef-migrate", Description = "Migrate sql server database using dotnet ef tool")]