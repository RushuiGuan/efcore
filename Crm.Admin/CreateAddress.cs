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
	[Verb<CreateAddress>("postgres create-address")]
	[Verb<CreateAddress>("sqlserver create-address")]
	public class CreateAddressParams {
		[Argument]
		public required string Company { get; init; }
		[Argument]
		public required string Contact { get; init; }
		[Option]
		public required string Line1 { get; init; }
	}
	public class CreateAddress : BaseHandler<CreateAddressParams> {
		private readonly ICrmRepository repository;
		public CreateAddress(ParseResult result, CreateAddressParams parameters, ICrmRepository repository) : base(result, parameters) {
			this.repository = repository;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = await this.repository.GetContactByName(parameters.Company, parameters.Contact, cancellationToken)
			              ?? throw new NotFoundException<Contact>(parameters.Company, parameters.Contact);
			var address = new Address {
				Id = Guid.NewGuid(),
				ContactId = contact.Id,
				Line1 = parameters.Line1,
			};
			repository.Add(address);
			await this.repository.SaveChangesAsync(cancellationToken);
			return 0;
		}
	}
}