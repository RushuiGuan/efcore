using Sample.Models;
using Sql = Albatross.EFCore.SqlServer;


namespace Sample.Admin {
	public class SampleSqlServerMigration : SampleDbSession {
		// need this constructor for the ef migration tool
		public SampleSqlServerMigration() : this("any") { }
		public SampleSqlServerMigration(string connectionString)
			: base(Sql.Extensions.BuildMigrationOption<SampleDbSession>(My.Schema.Sample, connectionString)) {
		}
	}
}