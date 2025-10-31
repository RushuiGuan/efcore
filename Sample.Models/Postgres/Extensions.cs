using Albatross.Config;
using Albatross.EFCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Models.Postgres {
	public static class Extensions {
		public static IServiceCollection AddPostresSampleDbSession(this IServiceCollection services) {
			services.AddConfig<SampleConfig>();
			services.AddSampleDbSession();
			services.AddPostgresWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<SampleConfig>().ConnectionString);
			return services;
		}
	}
}