using Albatross.Authentication.Windows;
using Albatross.CommandLine;
using Albatross.Config;
using Albatross.EFCore.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Models;
using System.CommandLine.Invocation;
using System.Linq;

namespace Sample.Admin {
	public class MySetup : Setup {
		public override void RegisterServices(InvocationContext context, IConfiguration configuration, EnvironmentSetting envSetting, IServiceCollection services) {
			base.RegisterServices(context, configuration, envSetting, services);
			services.AddWindowsPrincipalProvider();
			services.AddSampleDbSession();

			var command = context.ParseResult.CommandResult.Command;
			if (command.Name == "sqlserver" || command.Parents.FirstOrDefault()?.Name == "sqlserver") {
				services.AddConfig<ISampleConfig, Models.SqlServer.SampleConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteSqlServerScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal SampleDbSession
				services.AddScoped(provider => {
					return new SampleSqlServerMigration(provider.GetRequiredService<ISampleConfig>().ConnectionString);
				});
			} else {
				services.AddConfig<ISampleConfig, Models.Postgres.SampleConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal SampleDbSession
				services.AddScoped(provider => {
					return new SamplePostgresMigration(provider.GetRequiredService<ISampleConfig>().ConnectionString);
				});
			}
			services.RegisterCommands();
		}
	}
}