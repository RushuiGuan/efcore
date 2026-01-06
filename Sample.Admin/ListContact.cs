using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.Expression.Nodes;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Sample.Models;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb<ListContact>("postgres contact list", Description = "List all existing contacts")]
	[Verb<ListContact>("sqlserver contact list", Description = "List all existing contacts")]
	public class ListContactParams {
		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}
	public class ListContact : BaseHandler<ListContactParams> {
		private readonly ISampleDbSession session;

		public ListContact(ListContactParams options, ParseResult result, ISampleDbSession session) : base(result, options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var items = await this.session.DbContext.Set<Contact>().ToArrayAsync(cancellationToken);
			this.Writer.CliPrintWithExpression(items, parameters.Format);
			return 0;
		}
	}
}
