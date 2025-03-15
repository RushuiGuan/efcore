using Albatross.CommandLine;
using Albatross.EFCore;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres ef-migrate", typeof(PostgresEFMigrate), Description = "Migrate postgres database using dotnet ef tool")]
	public class PostgresEFMigrationOptions { }

	public class PostgresEFMigrate : BaseHandler<PostgresEFMigrationOptions> {
		private readonly Migration<SampleSqlMigration> svc;

		public PostgresEFMigrate(Migration<SampleSqlMigration> svc, IOptions<PostgresEFMigrationOptions> options) : base(options) {
			this.svc = svc;
		}
		public override async Task<int> InvokeAsync(InvocationContext context) {
			await svc.MigrateEfCore();
			return 0;
		}
	}
}