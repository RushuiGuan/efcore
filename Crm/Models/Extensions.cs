using Albatross.EFCore.ChangeReporting;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Models {
	public static class Extensions {
		public static IServiceCollection AddCrmDbSession(this IServiceCollection services) {
			services.AddScoped<ICrmDbSession>(provider => provider.GetRequiredService<CrmDbSession>())
				.AddScoped<ICompanyRepository, CompanyRepository>();
			return services;
		}

		public static IServiceCollection AddChangeReporting(this IServiceCollection services) {
			services.AddChangeReporting(new ChangeReportBuilder<Contact> {
				Type = ChangeType.All,
				Prefix = "``` start\n",
				Postfix = "```",
				FixedHeaders = [nameof(Contact.Id)],
				OnReportGenerated = text => Console.Out.WriteLineAsync(text)
			}.ExcludeAuditProperties());

			services.AddChangeReporting(new ChangeReportBuilder<Address> {
				Type = ChangeType.All,
				Prefix = "``` start\n",
				Postfix = "```",
				FixedHeaders = [nameof(Address.Id)],
				OnReportGenerated = text => Console.Out.WriteLineAsync(text)
			});
			return services;
		}
	}
}