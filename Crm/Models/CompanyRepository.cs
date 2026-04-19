using Albatross.EFCore;
using Albatross.EFCore.PostgreSQL;
using Albatross.EFCore.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Crm.Models {
	public interface ICompanyRepository : IRepository {
		Task<List<Company>> GetAll(CancellationToken cancellationToken);
		Task<Company?> GetById(Guid id, CancellationToken cancellationToken);
		Task<Company?> GetByName(string name, CancellationToken cancellationToken);
	}

	public class CompanyRepository : Repository<ICrmDbSession>, ICompanyRepository {
		public CompanyRepository(ICrmDbSession session) : base(session) { }

		public override bool IsUniqueConstraintViolation(Exception err) =>
			SqlServerExceptionExtensions.IsUniqueConstraintViolation(err) || PostgresExceptionExtensions.IsUniqueConstraintViolation(err);

		public override bool IsForeignKeyConstraintViolation(Exception err) =>
			SqlServerExceptionExtensions.IsForeignKeyConstraintViolation(err) || PostgresExceptionExtensions.IsForeignKeyConstraintViolation(err);

		public Task<List<Company>> GetAll(CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().ToListAsync(cancellationToken);

		public Task<Company?> GetById(Guid id, CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		public Task<Company?> GetByName(string name, CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
	}
}