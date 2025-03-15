using Albatross.CommandLine;
using Albatross.EFCore;
using Microsoft.Extensions.Options;
using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Sample.Admin {
	[Verb("postgres exec-script", typeof(ExecutePostgreslDeploymentScript), Description = "Execute postgres deployment scripts")]
	public class ExecutePostgresDeploymentScriptOptions {
		[Argument(Description = "The directory where the scripts are located")]
		public string Location { get; set; } = String.Empty;
	}

	public class ExecutePostgreslDeploymentScript : BaseHandler<ExecutePostgresDeploymentScriptOptions> {
		private readonly Migration<SamplePostgresMigration> svc;

		public ExecutePostgreslDeploymentScript(Migration<SamplePostgresMigration> svc, IOptions<ExecutePostgresDeploymentScriptOptions> options) : base(options) {
			this.svc = svc;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			await svc.ExecuteDeploymentScript(options.Location);
			return 0;
		}
	}
}