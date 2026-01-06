using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.Expression.Nodes;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Sample.Models;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb<NewAddress>("postgres address new", Description = "Create a new address")]
	[Verb<NewAddress>("sqlserver address new", Description = "Create a new address")]
	public class NewAddressOptions {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		public string? Line1 { get; set; }
		public string? Line2 { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? PostalCode { get; set; }

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}
	public class NewAddress : BaseHandler<NewAddressOptions> {
		private readonly ISampleDbSession session;

		public NewAddress(NewAddressOptions parameters, ParseResult result, ISampleDbSession session) : base(result, parameters) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = await this.session.DbContext.Set<Contact>()
				.FirstOrDefaultAsync(x => x.Name == this.parameters.Name) ?? throw new ArgumentException($"{parameters.Name} is not a existing contact name");

			var address = new Address {
				Line1 = parameters.Line1,
				Line2 = parameters.Line2,
				City = parameters.City,
				State = parameters.State,
				PostalCode = parameters.PostalCode,
				ContactId = contact.Id
			};
			this.session.DbContext.Set<Address>().Add(address);
			await this.session.SaveChangesAsync(cancellationToken);
			this.Writer.CliPrintWithExpression(address, parameters.Format);
			return 0;
		}
	}
}
