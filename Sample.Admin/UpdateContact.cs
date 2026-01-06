using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.Expression.Nodes;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Sample.Models;
using System;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb<UpdateContact>("postgres contact edit", Description = "Edit an existing contact")]
	[Verb<UpdateContact>("sqlserver contact edit", Description = "Edit an existing contact")]
	public class UpdateContactParams {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		[Option("new-name", Description = "New contact name")]
		public string NewName { get; set; } = string.Empty;

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}
	public class UpdateContact : BaseHandler<UpdateContactParams> {
		private readonly ISampleDbSession session;

		public UpdateContact(UpdateContactParams parameters, ParseResult result, ISampleDbSession session) : base(result, parameters) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = await this.session.DbContext.Set<Contact>().Where(x=>x.Name == parameters.Name).FirstOrDefaultAsync() 
				?? throw new ArgumentException($"'{parameters.Name}' is not a valid contact name");
			contact.Name = parameters.NewName;
			await this.session.SaveChangesAsync();
			this.Writer.CliPrintWithExpression(contact, parameters.Format);
			return 0;
		}
	}
}
