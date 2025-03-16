using Albatross.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class ExecuteDeploymentScriptsOptions {
		[Argument(Description = "The directory where the scripts are located")]
		public string Directory { get; set; } = string.Empty;

		[Option("pre", Description = "If true, the scripts should be executed prior the ef migration.  They should be skipped if there are no pending migrations")]
		public bool PreMigration { get; set; }
	}

	public class ExecuteDeploymentScripts<T> : BaseHandler<ExecuteDeploymentScriptsOptions> where T : IDbSession {
		private readonly IExecuteScriptFile executeScriptFile;
		private readonly T session;
		private readonly ILogger logger;
		
		static string FindTargetVersionScriptLocation(string directory) {
			var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
			if (string.IsNullOrEmpty(version)) {
				version = "UnknownVersion";
			} else {
				var ver = new Albatross.SemVer.SematicVersion(version) {
					PreRelease = [],
					Metadata = []
				};
				version = ver.ToString();
			}
			return Path.Join(directory, version);
		}
		

		public ExecuteDeploymentScripts(IExecuteScriptFile executeScriptFile, T session, ILogger logger, IOptions<ExecuteDeploymentScriptsOptions> options) : base(options) {
			this.executeScriptFile = executeScriptFile;
			this.session = session;
			this.logger = logger;
		}

		public override async Task<int> InvokeAsync(InvocationContext context) {
			if (options.PreMigration && !this.session.DbContext.Database.HasPendingModelChanges()) {
				logger.LogInformation("Skip pre-migration script since there is no pending model changes");
				return 0;
			} else {
				var directory = FindTargetVersionScriptLocation(options.Directory);
				logger.LogInformation("Executing deployment script at {location}", directory);
				await this.executeScriptFile.ExecuteAsync(this.session.DbContext, directory);
				return 0;
			}
		}
	}
}