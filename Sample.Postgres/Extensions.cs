using Albatross.Config;
using Albatross.EFCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Sample.Models;

namespace Sample.Postgres {
	public static class Extensions {
		public static IServiceCollection AddPostresSampleDbSession(this IServiceCollection services) {
			services.AddConfig<SampleConfig>();
			services.AddSampleDbSession();
			services.AddPostgresWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<SampleConfig>().ConnectionString);
			return services;
		}
	}
}