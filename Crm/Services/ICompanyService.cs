using Crm.Models;
using Crm.Requests;

namespace Crm.Services {
	public interface ICompanyService {
		Company CreateNewCompany(CreateCompanyRequest request);
		Task<Company> UpdateCompany(string name, UpdateCompanyRequest request, CancellationToken cancellationToken = default);
	}

	public class CompanyService : ICompanyService {
		readonly ICompanyRepository repo;

		public CompanyService(ICompanyRepository repo) {
			this.repo = repo;
		}

		public Company CreateNewCompany(CreateCompanyRequest request) {
			var company = new Company {
				Id = Guid.NewGuid(),
				Name = request.Name,
				Description = request.Description,
			};
			repo.Add(company);
			return company;
		}

		public async Task<Company> UpdateCompany(string name, UpdateCompanyRequest request, CancellationToken cancellationToken = default) {
			var company = await repo.GetByName(name, cancellationToken)
				?? throw new InvalidOperationException($"Company '{name}' not found");
			if (request.NewName != null) {
				company.Name = request.NewName;
			}
			company.Description = request.Description;
			return company;
		}
	}
}
