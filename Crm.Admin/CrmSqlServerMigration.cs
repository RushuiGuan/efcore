using Crm.Models;
using Sql = Albatross.EFCore.SqlServer;


namespace Crm.Admin {
	public class CrmSqlServerMigration : CrmDbSession {
		// need this constructor for the ef migration tool
		public CrmSqlServerMigration() : this("any") { }
		public CrmSqlServerMigration(string connectionString)
			: base(Sql.Extensions.BuildMigrationOption<CrmDbSession>(Constants.Schema, connectionString)) {
		}
	}
}