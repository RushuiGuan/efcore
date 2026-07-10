using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Crm.Repositories;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<DeleteCompany>("postgres delete-company")]
	[Verb<DeleteCompany>("sqlserver delete-company")]
	public class DeleteCompanyParams {
		[Argument]
		public required string Name { get; init; }
	}
	public class DeleteCompany : BaseHandler<DeleteCompanyParams> {
		private readonly ICrmRepository repository;

		public DeleteCompany(ICrmRepository repository, ParseResult result, DeleteCompanyParams parameters) : base(result, parameters) {
			this.repository = repository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var company = await repository.GetCompanyByName(parameters.Name, cancellationToken);
			if (company != null) {
				this.repository.Delete(company);
			}
			await this.repository.SaveChangesAsync(cancellationToken);
			return 0;
		}
	}
}