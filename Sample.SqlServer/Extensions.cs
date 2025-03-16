using Albatross.Config;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Sample.Models;

namespace Sample.SqlServer;

public static class Extensions {
	public static IServiceCollection AddSqlServerSampleDbSession(this IServiceCollection services) {
		services.AddConfig<SampleConfig>();
		services.AddSampleDbSession();
		return services.AddSqlServerWithContextPool<SampleDbSession>(provider => provider.GetRequiredService<SampleConfig>().ConnectionString);
	}
}