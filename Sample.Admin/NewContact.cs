using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.Extensions.Options;
using Sample.Models;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres contact new", typeof(NewContact), Description = "Create a new contact")]
	[Verb("sqlserver contact new", typeof(NewContact), Description = "Create a new contact")]
	public class NewContactOptions {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}
	public class NewContact : BaseHandler<NewContactOptions> {
		private readonly ISampleDbSession session;

		public NewContact(IOptions<NewContactOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var contact = new Contact {
				Name = this.options.Name
			};
			this.session.DbContext.Set<Contact>().Add(contact);
			await this.session.SaveChangesAsync();
			this.writer.CliPrint(contact, options.Format);
			return 0;
		}
	}
}
