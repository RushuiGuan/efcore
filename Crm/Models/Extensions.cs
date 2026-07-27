using Albatross.EFCore;
using Crm.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crm.Models {
	public static class Extensions {
		public static IServiceCollection AddCrmDbSession(this IServiceCollection services) {
			services.AddScoped<ICrmDbSession>(provider => provider.GetRequiredService<CrmDbSession>())
				.AddScoped<ICrmRepository, CrmRepository>();
			services.TryAddSingleton<ChangeAuditInterceptor<Audit, Guid, Guid>>();
			services.TryAddSingleton<TimeProvider>(TimeProvider.System);
			return services;
		}
		
		public static DbContextOptionsBuilder AddCrmInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider) {
			optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<ChangeAuditInterceptor<Audit, Guid, Guid>>());
			return optionsBuilder;
		}
	}
}