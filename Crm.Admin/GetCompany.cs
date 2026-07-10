using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Crm.Repositories;
using Crm.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<GetCompany>("postgres get-company")]
	[Verb<GetCompany>("sqlserver get-company")]
	public class GetCompanyParams {
		[Argument]
		public required string Name { get; init; }
	}
	public class GetCompany : BaseHandler<GetCompanyParams> {
		private readonly ICompanyService service;
		private readonly ICrmRepository repository;

		public GetCompany(ICompanyService service, ICrmRepository repository, ParseResult result, GetCompanyParams parameters) : base(result, parameters) {
			this.service = service;
			this.repository = repository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var company = await repository.GetCompanyByName(parameters.Name, cancellationToken);
			if (company != null) {
				company.Print(null, false);
			}
			return 0;
		}
	}
}