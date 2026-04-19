using Crm.Models;
using Albatross.Config;
using Albatross.EFCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Models.SqlServer;

public static class Extensions {
	public static IServiceCollection AddSqlServerSampleDbSession(this IServiceCollection services) {
		services.AddConfig<CrmConfig>();
		services.AddCrmDbSession();
		return services.AddSqlServerWithContextPool<CrmDbSession>(provider => provider.GetRequiredService<CrmConfig>().ConnectionString);
	}
}