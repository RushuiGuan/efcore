using Albatross.CommandLine;
using Albatross.EFCore;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("sql ef-migrate", typeof(SqlServerEFMigrate), Description = "Migrate sql database using dotnet ef tool")]
	public class SqlServerEFMigrationOptions { }

	public class SqlServerEFMigrate : BaseHandler<SqlServerEFMigrationOptions> {
		private readonly Migration<SampleSqlMigration> svc;

		public SqlServerEFMigrate(Migration<SampleSqlMigration> svc, IOptions<SqlServerEFMigrationOptions> options) : base(options) {
			this.svc = svc;
		}
		public override async Task<int> InvokeAsync(InvocationContext context) {
			await svc.MigrateEfCore();
			return 0;
		}
	}
}