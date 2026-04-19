using Crm.Models;

namespace Crm.Admin {
	public class CrmPostgresMigration : CrmDbSession {
		// need this constructor for the ef migration tool
		public CrmPostgresMigration() : this("any") { }

		public CrmPostgresMigration(string connectionString)
			: base(Albatross.EFCore.PostgreSQL.Extensions.BuildPostgresMigrationDbContext<CrmDbSession>(Constants.Schema, connectionString)) {
		}
	}
}