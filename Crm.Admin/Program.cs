using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Albatross.EFCore;
using Albatross.Config;
using Albatross.EFCore.Admin;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Albatross.Logging;
using Crm.Models;
using Crm.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Crm.Admin {
	internal class Program {
		static async Task<int> Main(string[] args) {
			Albatross.Logging.Extensions.RemoveLegacySlackSinkOptions();
			if (Microsoft.EntityFrameworkCore.EF.IsDesignTime) {
				await new HostBuilder().Build().RunAsync();
				return 0;
			} else {
				await using var host = new CommandHost("Crm Admin Program")
					.RegisterServices(RegisterServices)
					.AddCommands()
					.Parse(args)
					.WithConfig()
					.ConfigureHost(builder => {
						builder.UseSerilog();
						builder.ConfigureLogging((context, logging) => {
							var setupSerilog = new SetupSerilog();
							setupSerilog.UseConfigFile(EnvironmentSetting.DOTNET_ENVIRONMENT.Value, null, null, true);
							setupSerilog.Create();
						});
					})
					.Build();
				return await host.InvokeAsync();
			}
		}
		static void RegisterServices(ParseResult result, IServiceCollection services) {
			services.AddCrmDbSession().AddCrm();
			services.AddSingleton<IGetCurrentActorId<Guid>, SystemActorId>();
			var key = result.CommandResult.Command.GetCommandKey();
			if (key.StartsWith("sqlserver")) {
				services.AddConfig<ICrmConfig, Crm.Models.SqlServer.CrmConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteSqlServerScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal CrmDbSession
				services.AddScoped(provider => new CrmSqlServerMigration(provider.GetRequiredService<ICrmConfig>().ConnectionString));
				services.AddSqlServer<CrmDbSession>(provider => provider.GetRequiredService<ICrmConfig>().ConnectionString, true, (builder, sp) => builder.AddCrmInterceptors(sp));
			} else if (key.StartsWith("postgres")) {
				services.AddConfig<ICrmConfig, Crm.Models.Postgres.CrmConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal CrmDbSession
				services.AddScoped(provider => new CrmPostgresMigration(provider.GetRequiredService<ICrmConfig>().ConnectionString));
				services.AddPostgres<CrmDbSession>(provider => provider.GetRequiredService<ICrmConfig>().ConnectionString, true, (builder, sp) => builder.AddCrmInterceptors(sp));
			}
			services.RegisterCommands();
		}
	}
}