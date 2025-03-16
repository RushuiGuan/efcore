using Albatross.CommandLine;
using Albatross.EFCore.Admin;
using Sample.Admin;

[assembly: Verb("postgres exec-script", typeof(ExecuteDeploymentScripts<SamplePostgresMigration>), OptionsClass = typeof(ExecuteDeploymentScriptsOptions), Description = "Execute postgres deployment scripts")]
[assembly: Verb("postgres create-script", typeof(GenerateSqlScript<SamplePostgresMigration>), OptionsClass = typeof(GenerateSqlScriptOptions), Description = "Generate sql script for postgres")]
[assembly: Verb("postgres ef-migrate", typeof(EFMigrate<SamplePostgresMigration>), OptionsClass = typeof(EFMigrationOptions), Description = "Migrate postgres database using dotnet ef tool")]
	
	
[assembly: Verb("sql exec-script", typeof(ExecuteDeploymentScripts<SampleSqlMigration>), OptionsClass = typeof(ExecuteDeploymentScriptsOptions), Description = "Execute sql server deployment scripts")]
[assembly: Verb("sql create-script", typeof(GenerateSqlScript<SampleSqlMigration>), OptionsClass = typeof(GenerateSqlScriptOptions), Description = "Generate sql script for sql server")]
[assembly: Verb("sql ef-migrate", typeof(EFMigrate<SampleSqlMigration>), OptionsClass = typeof(EFMigrationOptions), Description = "Migrate sql server database using dotnet ef tool")]