using Albatross.EFCore;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Albatross.Exceptions;
using Crm.Models;
using Microsoft.EntityFrameworkCore;

namespace Crm.Repositories {
	public partial interface ICrmRepository : IRepository {
		Task<List<Company>> GetAllCompany(CancellationToken cancellationToken);
		Task<Company?> GetByCompanyId(Guid id, CancellationToken cancellationToken);
		Task<Company?> GetCompanyByName(string name, CancellationToken cancellationToken);
		Task<Contact?> GetContactByName(string company, string name, CancellationToken cancellationToken);
	}

	public class CrmRepository : Repository<ICrmDbSession>, ICrmRepository {
		public CrmRepository(ICrmDbSession session, ISemanticExceptionConverter exceptionConverter) : base(session, exceptionConverter) { }

		public Task<List<Company>> GetAllCompany(CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().ToListAsync(cancellationToken);

		public Task<Company?> GetByCompanyId(Guid id, CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		public Task<Company?> GetCompanyByName(string name, CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

		public Task<Contact?> GetContactByName(string company, string name, CancellationToken cancellationToken)
			=> session.DbContext.Set<Contact>().Where(x => x.Name == name && x.Company.Name == company).FirstOrDefaultAsync(cancellationToken);
	}
}