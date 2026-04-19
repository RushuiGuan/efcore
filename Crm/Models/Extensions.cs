using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Models {
	public static class Extensions {
		public static IServiceCollection AddCrmDbSession(this IServiceCollection services) {
			services.AddScoped<ICrmDbSession>(provider => provider.GetRequiredService<CrmDbSession>())
				.AddScoped<ICompanyRepository, CompanyRepository>();
			services.AddSingleton<ChangeAuditInterceptor<Audit, Guid, Guid>>();
			return services;
		}
		
		public static DbContextOptionsBuilder AddAuditInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider) {
			optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<ChangeAuditInterceptor<Audit, Guid, Guid>>());
			return optionsBuilder;
		}
	}
}