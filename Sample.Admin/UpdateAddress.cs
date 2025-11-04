using Albatross.CommandLine;
using Albatross.Text.CliFormat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.Models;
using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres address edit", typeof(UpdateAddress), Description = "Edit an existing address")]
	[Verb("sqlserver address edit", typeof(UpdateAddress), Description = "Edit an existing address")]
	public class UpdateAddressOptions {
		[Argument(Description = "Address id")]
		public int Id { get; set; }

		public string? Line1 { get; set; }
		public string? Line2 { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? PostalCode { get; set; }

		[Option("f", Description = "Output format expression")]
		public string? Format { get; set; }
	}
	public class UpdateAddress : BaseHandler<UpdateAddressOptions> {
		private readonly ISampleDbSession session;

		public UpdateAddress(IOptions<UpdateAddressOptions> options, ISampleDbSession session) : base(options) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			var address = await this.session.DbContext.Set<Address>().FirstOrDefaultAsync(x => x.Id == options.Id)
				?? throw new ArgumentException($"'{options.Id}' is not a valid address id");
			if (!string.IsNullOrEmpty(options.Line1)) {
				address.Line1 = options.Line1;
			}
			if (!string.IsNullOrEmpty(options.Line2)) {
				address.Line2 = options.Line2;
			}
			if (!string.IsNullOrEmpty(options.City)) {
				address.City = options.City;
			}
			if (!string.IsNullOrEmpty(options.State)) {
				address.State = options.State;
			}
			if (!string.IsNullOrEmpty(options.PostalCode)) {
				address.PostalCode = options.PostalCode;
			}
			await this.session.SaveChangesAsync();
			this.writer.CliPrint(address, options.Format);
			return 0;
		}
	}
}
