using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.Models;
using System;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres edit-contact", typeof(UpdateContact), Description = "Edit an existing contact")]
	[Verb("sqlserver edit-contact", typeof(UpdateContact), Description = "Edit an existing contact")]
	public class UpdateContactOptions {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		[Option("new-name", Description = "New contact name")]
		public string NewName { get; set; } = string.Empty;

		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}
	public class UpdateContact : BaseHandler<UpdateContactOptions> {
		private readonly ISampleDbSession session;

		public UpdateContact(IOptions<UpdateContactOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var contact = await this.session.DbContext.Set<Contact>().Where(x=>x.Name == options.Name).FirstOrDefaultAsync() 
				?? throw new ArgumentException($"'{options.Name}' is not a valid contact name");
			contact.Name = options.NewName;
			await this.session.SaveChangesAsync();
			this.writer.CliPrint(contact, options.Format);
			return 0;
		}
	}
}
