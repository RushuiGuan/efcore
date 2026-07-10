using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.EFCore;
using Crm.Models;
using Crm.Repositories;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<DeleteContact>("postgres delete-contact")]
	[Verb<DeleteContact>("sqlserver delete-contact")]
	public class DeleteContactParams {
		[Argument]
		public required string Company { get; init; }
		[Argument]
		public required string Name { get; init; }
	}
	public class DeleteContact : BaseHandler<DeleteContactParams> {
		private readonly ICrmRepository repository;

		public DeleteContact(ICrmRepository repository, ParseResult result, DeleteContactParams parameters) : base(result, parameters) {
			this.repository = repository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = await repository.GetContactByName(parameters.Company, parameters.Name, cancellationToken)
			              ?? throw new NotFoundException<Contact>(parameters.Company, parameters.Name);
			this.repository.Delete(contact);
			await this.repository.SaveChangesAsync(cancellationToken);
			return 0;
		}
	}
}