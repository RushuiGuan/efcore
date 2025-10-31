using Albatross.EFCore;
using Albatross.EFCore.Audit;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Models {
	public static class Extensions {
		public static IServiceCollection AddSampleDbSession(this IServiceCollection services) {
			services.AddDbSessionEvents()
				.AddAuditEventHandlers()
			// .AddChangeReports();
			// .AddAutoCacheEviction()
			.AddSqlServerWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<ISampleConfig>().ConnectionString)
			.AddScoped<ISampleDbSession>(provider => provider.GetRequiredService<SampleDbSession>());
			return services;
		}
	}
}