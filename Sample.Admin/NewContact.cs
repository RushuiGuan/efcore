using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.Expression.Nodes;
using Albatross.Text.CliFormat;
using Sample.Models;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb<NewContact>("postgres contact new", Description = "Create a new contact")]
	[Verb<NewContact>("sqlserver contact new", Description = "Create a new contact")]
	public class NewContactParams {
		[Argument(Description = "Contact name")]
		public string Name { get; set; } = string.Empty;

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}
	public class NewContact : BaseHandler<NewContactParams> {
		private readonly ISampleDbSession session;

		public NewContact(NewContactParams options, ParseResult results, ISampleDbSession session) : base(results, options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var contact = new Contact {
				Name = this.parameters.Name
			};
			this.session.DbContext.Set<Contact>().Add(contact);
			await this.session.SaveChangesAsync(cancellationToken);
			this.Writer.CliPrintWithExpression(contact, parameters.Format);
			return 0;
		}
	}
}
