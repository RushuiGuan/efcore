using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.Models;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres contact list", typeof(ListContact), Description = "List all existing contacts")]
	[Verb("sqlserver contact list", typeof(ListContact), Description = "List all existing contacts")]
	public class ListContactOptions {
		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}
	public class ListContact : BaseHandler<ListContactOptions> {
		private readonly ISampleDbSession session;

		public ListContact(IOptions<ListContactOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var items  = await this.session.DbContext.Set<Contact>().ToArrayAsync();
			this.writer.CliPrint(items, options.Format);
			return 0;
		}
	}
}
