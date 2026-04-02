using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.Expression.Nodes;
using Albatross.Text.CliFormat;
using Crm.Models;
using Crm.Requests;
using Crm.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Admin {
	[Verb<CreateCompany>("postgres create-company", Description = "Create a new company")]
	[Verb<CreateCompany>("sqlserver create-company", Description = "Create a new company")]
	public class CreateCompanyParams {
		[Option]
		public required string Name { get; init; }
		[Option]
		public required string? Description { get; init; }

		[UseOption<FormatExpressionOption>]
		public IExpression? Format { get; init; }
	}
	public class CreateCompany : BaseHandler<CreateCompanyParams> {
		private readonly ICompanyService service;
		private readonly ICompanyRepository repository;

		public CreateCompany(ICompanyService service, ICompanyRepository repository, ParseResult result, CreateCompanyParams parameters) : base(result, parameters) {
			this.service = service;
			this.repository = repository;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var request = new CreateCompanyRequest {
				Name = parameters.Name,
				Description = parameters.Description
			};
			var company = service.CreateNewCompany(request);
			var saved = await repository.SaveChangesAsync(false, cancellationToken);
			if (saved.Success) {
				this.Writer.CliPrintWithExpression(company, parameters.Format);
				return 0;
			} else {
				if (saved.NameConflict) {
					result.InvocationConfiguration.Error.WriteLine("Company name already exists.");
				} else {
					result.InvocationConfiguration.Error.WriteLine("Failed to create company: " + saved.Error?.Message);
				}
				return 1;
			}
		}
	}
}
