using Sample.Models;

namespace Sample.Admin {
	public class SamplePostgresMigration : SampleDbSession {
		// need this constructor for the ef migration tool
		public SamplePostgresMigration() : this("any") { }

		public SamplePostgresMigration(string connectionString)
			: base(Albatross.EFCore.PostgreSQL.Extensions.BuildMigrationOption<SampleDbSession>(My.Schema.Sample, connectionString)) {
		}
	}
}