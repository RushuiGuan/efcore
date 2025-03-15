using Albatross.CommandLine;
using Albatross.EFCore.SqlServer;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("sql exec-script", typeof(ExecuteSqlDeploymentScript), Description = "Execute sql deployment scripts")]
	public class ExecuteSqlDeploymentScriptOptions {
		[Argument(Description = "The directory where the scripts are located")]
		public string Location { get; set; } = string.Empty;
	}

	public class ExecuteSqlDeploymentScript : BaseHandler<ExecuteSqlDeploymentScriptOptions> {
		private readonly SqlServerMigration<SampleSqlMigration> svc;

		public ExecuteSqlDeploymentScript(SqlServerMigration<SampleSqlMigration> svc, IOptions<ExecuteSqlDeploymentScriptOptions> options) : base(options) {
			this.svc = svc;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			await svc.ExecuteDeploymentScript(options.Location);
			return 0;
		}
	}
}