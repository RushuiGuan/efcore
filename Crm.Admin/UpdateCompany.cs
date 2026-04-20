using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Crm.Models;
using Crm.Repositories;
using Crm.Requests;
using Crm.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<UpdateCompanyHandler>("update-company", Description = "Update a company's name and description")]
	public record class UpdateCompanyParams {
		[Argument(Description = "Current name of the company to update")]
		public required string Name { get; init; }

		[Option(Description = "New name for the company")]
		public string? NewName { get; init; }

		[Option(Description = "New description for the company")]
		public string? Description { get; init; }
	}

	public class UpdateCompanyHandler : BaseHandler<UpdateCompanyParams> {
		readonly ICompanyService companyService;
		readonly ICrmRepository crmRepository;

		public UpdateCompanyHandler(ParseResult result, UpdateCompanyParams parameters,
			ICompanyService companyService, ICrmRepository crmRepository)
			: base(result, parameters) {
			this.companyService = companyService;
			this.crmRepository = crmRepository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var request = new UpdateCompanyRequest {
				NewName = parameters.NewName,
				Description = parameters.Description,
			};
			var company = await companyService.UpdateCompany(parameters.Name, request, cancellationToken);
			await crmRepository.SaveChangesAsync(true, cancellationToken);
			Writer.WriteLine($"Updated: {company.Name}");
			return 0;
		}
	}
}
