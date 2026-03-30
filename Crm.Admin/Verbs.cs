using Albatross.CommandLine.Annotations;
using Albatross.EFCore.Admin;
using Crm.Admin;

[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<CrmPostgresMigration>>("postgres exec-script", Description = "Execute postgres deployment scripts")]
[assembly: Verb<GenerateSqlScriptParams, GenerateSqlScript<CrmPostgresMigration>>("postgres create-script", Description = "Generate sql script for postgres")]
[assembly: Verb<EFMigrationParams, EFMigrate<CrmPostgresMigration>>("postgres ef-migrate", Description = "Migrate postgres database using dotnet ef tool")]


[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<CrmSqlServerMigration>>("sqlserver exec-script", Description = "Execute sql server deployment scripts")]
[assembly: Verb<GenerateSqlScriptParams, GenerateSqlScript<CrmSqlServerMigration>>("sqlserver create-script", Description = "Generate sql script for sql server")]
[assembly: Verb<EFMigrationParams, EFMigrate<CrmSqlServerMigration>>("sqlserver ef-migrate", Description = "Migrate sql server database using dotnet ef tool")]