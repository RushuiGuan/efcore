using Sample.Models;
using Sql = Albatross.EFCore.SqlServer;


namespace Sample.Admin {
	public class SampleSqlMigration : SampleDbSession {
		public SampleSqlMigration() : this("any") { }
		public SampleSqlMigration(string connectionString)
			: base(Sql.Extensions.BuildMigrationOption<SampleDbSession>(My.Schema.Sample, connectionString)) {
		}
	}
}