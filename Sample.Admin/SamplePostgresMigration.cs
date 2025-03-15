using Sample.Models;

namespace Sample.Admin {
	public class SamplePostgresMigration : SampleDbSession {
		public SamplePostgresMigration() : this("any") {
		}
		public SamplePostgresMigration(string connectionString)
			: base(Albatross.EFCore.PostgreSQL.Extensions.BuildMigrationOption<SampleDbSession>(My.Schema.Sample, connectionString)) {}
	}
}