using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Crm.Repositories;
using Crm.Requests;
using Crm.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<CreateCompany>("postgres create-company", Description = "Create a new company")]
	[Verb<CreateCompany>("sqlserver create-company", Description = "Create a new company")]
	public class CreateCompanyParams {
		[Option]
		public required string Name { get; init; }
		[Option]
		public required string? Description { get; init; }
	}
	public class CreateCompany : BaseHandler<CreateCompanyParams> {
		private readonly ICompanyService service;
		private readonly ICrmRepository repository;

		public CreateCompany(ICompanyService service, ICrmRepository repository, ParseResult result, CreateCompanyParams parameters) : base(result, parameters) {
			this.service = service;
			this.repository = repository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var request = new CreateCompanyRequest {
				Name = parameters.Name,
				Description = parameters.Description
			};
			var company = service.CreateNewCompany(request);
			await repository.SaveChangesAsync(cancellationToken);
			company.Print(null, false);
			return 0;
		}
	}
}