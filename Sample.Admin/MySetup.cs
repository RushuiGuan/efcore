using Albatross.CommandLine;
using Albatross.Config;
using Albatross.EFCore;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Postgres;
using Sample.SqlServer;
using System;
using System.CommandLine.Invocation;
using System.Linq;

namespace Sample.Admin {
	public class MySetup : Setup {
		public override void RegisterServices(InvocationContext context, IConfiguration configuration, EnvironmentSetting envSetting, IServiceCollection services) {
			base.RegisterServices(context, configuration, envSetting, services);
			var command = context.ParseResult.CommandResult.Command;
			if (command.Name == "sql" || command.Parents.FirstOrDefault()?.Name == "sql") {
				services.AddSqlServerSampleDbSession();
				services.AddScoped<SqlServerMigration<SampleSqlMigration>>();
				services.AddScoped<SampleSqlMigration>(provider => {
					var config = provider.GetRequiredService<SqlServer.SampleConfig>();
					return new SampleSqlMigration(config.ConnectionString);
				});
				services.AddScoped<ISqlBatchExecution, SqlBatchExecution>();
			} else {
				services.AddPostresSampleDbSession();
				services.AddScoped<Migration<SamplePostgresMigration>>();
				services.AddScoped<SamplePostgresMigration>(provider => {
					var config = provider.GetRequiredService<Postgres.SampleConfig>();
					return new SamplePostgresMigration(config.ConnectionString);
				});
			}

			services.RegisterCommands();
		}
	}
}