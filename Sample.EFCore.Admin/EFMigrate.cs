using Albatross.CommandLine;
using Albatross.EFCore;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.EFCore.Admin {
	[Verb("ef-migrate", typeof(EFMigrate), Description = "Migrate database using dotnet ef tool")]
	public class EFMigrationOption { }

	public class EFMigrate : BaseHandler<EFMigrationOption> {
		private readonly Migration<SampleSqlServerMigration> svc;

		public EFMigrate(Migration<SampleSqlServerMigration> svc, IOptions<EFMigrationOption> options) : base(options) {
			this.svc = svc;
		}
		public override async Task<int> InvokeAsync(InvocationContext context) {
			await svc.MigrateEfCore();
			return 0;
		}
	}
}