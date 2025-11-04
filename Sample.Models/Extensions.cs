using Albatross.EFCore;
using Albatross.EFCore.Audit;
using Albatross.EFCore.ChangeReporting;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Models {
	public static class Extensions {
		public static IServiceCollection AddSampleDbSession(this IServiceCollection services) {
			services.AddDbSessionEvents()
				.AddAuditEventHandlers()
				.AddChangeReporting()
				// .AddAutoCacheEviction()
				.AddScoped<ISampleDbSession>(provider => provider.GetRequiredService<SampleDbSession>());
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