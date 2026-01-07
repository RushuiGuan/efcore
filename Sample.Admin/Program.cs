using Albatross.Authentication;
using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Albatross.Config;
using Albatross.EFCore.Admin;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Models;
using System.CommandLine;
using System.Threading.Tasks;

namespace Sample.Admin {
	internal class Program {
		static async Task<int> Main(string[] args) {
			if (Microsoft.EntityFrameworkCore.EF.IsDesignTime) {
				await new HostBuilder().Build().RunAsync();
				return 0;
			} else {
				await using var host = new CommandHost("Sample EFCore Admin Program")
					.RegisterServices(RegisterServices)
					.AddCommands()
					.Parse(args)
					.WithDefaults()
					.Build();
				return await host.InvokeAsync();
			}
		}
		static void RegisterServices(ParseResult result, IServiceCollection services) {
			services.AddSingleton<IGetCurrentUser, GetTestUser>();
			services.AddSampleDbSession();

			var key = result.CommandResult.Command.GetCommandKey();
			if (key.StartsWith("sqlserver")) {
				services.AddConfig<ISampleConfig, Models.SqlServer.SampleConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteSqlServerScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal SampleDbSession
				services.AddScoped(provider => new SampleSqlServerMigration(provider.GetRequiredService<ISampleConfig>().ConnectionString));
				services.AddSqlServerWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<ISampleConfig>().ConnectionString);
			} else if (key.StartsWith("postgres")) {
				services.AddConfig<ISampleConfig, Models.Postgres.SampleConfig>();
				services.AddSingleton<IExecuteScriptFile, ExecuteScriptFile>();
				// this is only used by migration.  it has a diff DbContextOptions than the normal SampleDbSession
				services.AddScoped(provider => new SamplePostgresMigration(provider.GetRequiredService<ISampleConfig>().ConnectionString));
				services.AddPostgresWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<ISampleConfig>().ConnectionString);
			}
			services.RegisterCommands();
		}
	}
}