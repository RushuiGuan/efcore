using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class ExecuteDeploymentScriptsParams {
		[Argument(Description = "The directory where the scripts are located")]
		public string Directory { get; set; } = string.Empty;

		[Option("pre", Description = "If true, the scripts should be executed prior the ef migration.  They should be skipped if there are no pending migrations")]
		public bool PreMigration { get; set; }
	}

	public class ExecuteDeploymentScripts<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly IExecuteScriptFile executeScriptFile;
		private readonly T session;
		private readonly ILogger logger;
		private readonly ExecuteDeploymentScriptsParams parameters;

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
		

		public ExecuteDeploymentScripts(IExecuteScriptFile executeScriptFile, T session, ILogger logger, ExecuteDeploymentScriptsParams parameters) {
			this.executeScriptFile = executeScriptFile;
			this.session = session;
			this.logger = logger;
			this.parameters = parameters;
		}

		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			if (parameters.PreMigration && !this.session.DbContext.Database.HasPendingModelChanges()) {
				logger.LogInformation("Skip pre-migration script since there is no pending model changes");
				return 0;
			} else {
				var directory = FindTargetVersionScriptLocation(parameters.Directory);
				logger.LogInformation("Executing deployment scripts at {location}", directory);
				var info = new DirectoryInfo(directory);
				if (info.Exists) {
					foreach (var file in info.GetFiles("*.sql", SearchOption.TopDirectoryOnly)) {
						await this.executeScriptFile.ExecuteAsync(this.session.DbContext, file.FullName, cancellationToken);
					}
				}
				return 0;
			}
		}
	}
}