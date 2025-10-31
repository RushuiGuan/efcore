using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.Models;
using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres new-address", typeof(NewAddress), Description = "Create a new address")]
	[Verb("sqlserver new-address", typeof(NewAddress), Description = "Create a new address")]
	public class NewAddressOptions {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		public string? Line1 { get; set; }
		public string? Line2 { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? PostalCode { get; set; }

		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}
	public class NewAddress : BaseHandler<NewAddressOptions> {
		private readonly ISampleDbSession session;

		public NewAddress(IOptions<NewAddressOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var contact = await this.session.DbContext.Set<Contact>()
				.FirstOrDefaultAsync(x => x.Name == this.options.Name) ?? throw new ArgumentException($"{options.Name} is not a existing contact name");

			var address = new Address {
				Line1 = options.Line1,
				Line2 = options.Line2,
				City = options.City,
				State = options.State,
				PostalCode = options.PostalCode,
				ContactId = contact.Id
			};
			this.session.DbContext.Set<Address>().Add(address);
			await this.session.SaveChangesAsync();
			this.writer.CliPrint(address, options.Format);
			return 0;
		}
	}
}
