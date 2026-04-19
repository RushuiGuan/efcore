using Albatross.Config;
using Albatross.EFCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Models.Postgres {
	public static class Extensions {
		public static IServiceCollection AddPostresSampleDbSession(this IServiceCollection services) {
			services.AddConfig<CrmConfig>();
			services.AddCrmDbSession();
			services.AddPostgresWithContextPool<CrmDbSession>(provider => provider.GetRequiredService<CrmConfig>().ConnectionString, true,
				(builder, sp) => builder.AddCrmInterceptors(sp));
			return services;
		}
	}
}