using Albatross.Authentication;
using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Albatross.Config;
using Albatross.EFCore.Admin;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Crm.Models;
using System.CommandLine;
using System.Threading.Tasks;

namespace Crm.Admin {
	internal class Program {
		static async Task<int> Main(string[] args) {
			if (Microsoft.EntityFrameworkCore.EF.IsDesignTime) {
				await new HostBuilder().Build().RunAsync();
				return 0;
			} else {
				await using var host = new CommandHost("Crm Admin Program")
					.RegisterServices(RegisterServices)
					.AddCommands()
					.Parse(args)
					.WithDefaults()
					.Build();
				return await host.InvokeAsync();
			}
		}
		static void RegisterServices(ParseResult result, IServiceCollection services) {
			services.AddCrmDbSession();
			var key = result.CommandResult.Command.GetCommandKey();
			if (key.StartsWith("sqlserver")) {
				services.AddConfig<ICrmConfig, Crm.Models.SqlServer.CrmConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteSqlServerScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal CrmDbSession
				services.AddScoped(provider => new CrmSqlServerMigration(provider.GetRequiredService<ICrmConfig>().ConnectionString));
				services.AddSqlServerWithContextPool<CrmDbSession>(provider => provider.GetRequiredService<ICrmConfig>().ConnectionString);
			} else if (key.StartsWith("postgres")) {
				services.AddConfig<ICrmConfig, Crm.Models.Postgres.CrmConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal CrmDbSession
				services.AddScoped(provider => new CrmPostgresMigration(provider.GetRequiredService<ICrmConfig>().ConnectionString));
				services.AddPostgresWithContextPool<CrmDbSession>(provider => provider.GetRequiredService<ICrmConfig>().ConnectionString);
			}
			services.RegisterCommands();
		}
	}
}