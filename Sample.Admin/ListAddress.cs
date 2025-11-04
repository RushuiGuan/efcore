using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.Models;
using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres address list", typeof(ListAddress), Description = "List all addresses of an contact")]
	[Verb("sqlserver address list", typeof(ListAddress), Description = "List all addresses of an contact")]
	public class ListAddressOptions {
		[Argument(Description = "Contact name")]
		public string Contact { get; set; } = string.Empty;

		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}

	public class ListAddress : BaseHandler<ListAddressOptions> {
		private readonly ISampleDbSession session;

		public ListAddress(IOptions<ListAddressOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var items = await this.session.DbContext.Set<Contact>()
				            .Include(x => x.Addresses)
				            .FirstOrDefaultAsync(x => x.Name == options.Contact)
			            ?? throw new ArgumentException($"{options.Contact} is not a valid contact name");
			this.writer.CliPrint(items, options.Format);
			return 0;
		}
	}
}