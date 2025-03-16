using Albatross.Config;
using Albatross.EFCore;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Sample.EFCore.Models;

namespace Sample.EFCore {
	public static class Extensions {
		public static IServiceCollection AddSample(this IServiceCollection services) {
			services.AddDbSessionEvents();
			services.AddConfig<SampleConfig>();
			services.AddScoped<ISampleDbSession>(provider => provider.GetRequiredService<SampleDbSession>());
			services.AddScoped<MyDataService>();
			return services;
		}

		public static IServiceCollection AddSqlServer(this IServiceCollection services)
			=> services.AddSqlServerWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<SampleConfig>().ConnectionString);

		public static IServiceCollection AddPostgres(this IServiceCollection services)
			=> services.AddPostgresWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<SampleConfig>().ConnectionString);
	}
}