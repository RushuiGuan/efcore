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
	[Verb<UpdateAddress>("postgres address edit", Description = "Edit an existing address")]
	[Verb<UpdateAddress>("sqlserver address edit", Description = "Edit an existing address")]
	public class UpdateAddressParams {
		[Argument(Description = "Address id")]
		public int Id { get; set; }

		public string? Line1 { get; set; }
		public string? Line2 { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? PostalCode { get; set; }

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; set; }
	}
	public class UpdateAddress : BaseHandler<UpdateAddressParams> {
		private readonly ISampleDbSession session;

		public UpdateAddress(UpdateAddressParams options, ParseResult result, ISampleDbSession session) : base(result, options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var address = await this.session.DbContext.Set<Address>().FirstOrDefaultAsync(x => x.Id == parameters.Id)
				?? throw new ArgumentException($"'{parameters.Id}' is not a valid address id");
			if (!string.IsNullOrEmpty(parameters.Line1)) {
				address.Line1 = parameters.Line1;
			}
			if (!string.IsNullOrEmpty(parameters.Line2)) {
				address.Line2 = parameters.Line2;
			}
			if (!string.IsNullOrEmpty(parameters.City)) {
				address.City = parameters.City;
			}
			if (!string.IsNullOrEmpty(parameters.State)) {
				address.State = parameters.State;
			}
			if (!string.IsNullOrEmpty(parameters.PostalCode)) {
				address.PostalCode = parameters.PostalCode;
			}
			await this.session.SaveChangesAsync();
			this.Writer.CliPrintWithExpression(address, parameters.Format);
			return 0;
		}
	}
}
