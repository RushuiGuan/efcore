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
	[Verb<ListAddress>("postgres address list", Description = "List all addresses of an contact")]
	[Verb<ListAddress>("sqlserver address list", Description = "List all addresses of an contact")]
	public class ListAddressParams {
		[Argument(Description = "Contact name")]
		public string Contact { get; set; } = string.Empty;

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}

	public class ListAddress : BaseHandler<ListAddressParams> {
		private readonly ISampleDbSession session;

		public ListAddress(ListAddressParams parameters, ParseResult result, ISampleDbSession session) : base(result, parameters) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var items = await this.session.DbContext.Set<Contact>()
				            .Include(x => x.Addresses)
				            .FirstOrDefaultAsync(x => x.Name == parameters.Contact, cancellationToken)
			            ?? throw new ArgumentException($"{parameters.Contact} is not a valid contact name");
			this.Writer.CliPrintWithExpression(items, parameters.Format);
			return 0;
		}
	}
}