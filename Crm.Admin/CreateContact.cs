using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.EFCore;
using Crm.Models;
using Crm.Repositories;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<CreateContact>("postgres create-contact")]
	[Verb<CreateContact>("sqlserver create-contact")]
	public class CreateContactParams {
		[Argument]
		public required Guid CompanyId { get; init; }
		[Argument]
		public required string Name { get; init; }
	}
	public class CreateContact : BaseHandler<CreateContactParams> {
		private readonly ICrmRepository repository;
		public CreateContact(ParseResult result, CreateContactParams parameters, ICrmRepository repository) : base(result, parameters) {
			this.repository = repository;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = new Contact {
				Id = Guid.NewGuid(),
				CompanyId = parameters.CompanyId,
				Name = parameters.Name,
			};
			repository.Add(contact);
			await this.repository.SaveChangesAsync(cancellationToken);
			return 0;
		}
	}
}