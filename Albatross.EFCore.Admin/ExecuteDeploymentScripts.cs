using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class ExecuteDeploymentScriptsParams {
		[UseArgument<InputDirectoryArgument>]
		public required DirectoryInfo Directory { get; set; }

		[Option("pre", Description = "If true, the scripts should be executed prior the ef migration.  They should be skipped if there are no pending migrations")]
		public bool PreMigration { get; set; }
	}

	public class ExecuteDeploymentScripts<T> : BaseHandler<ExecuteDeploymentScriptsParams> where T : IDbSession {
		private readonly IExecuteScriptFile executeScriptFile;
		private readonly T session;

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


		public ExecuteDeploymentScripts(IExecuteScriptFile executeScriptFile, T session, ILogger<ExecuteDeploymentScripts<T>> logger,
			ParseResult result, ExecuteDeploymentScriptsParams parameters) : base(result, parameters) {
			this.executeScriptFile = executeScriptFile;
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			if (parameters.PreMigration && !this.session.DbContext.Database.HasPendingModelChanges()) {
				await this.Writer.WriteLineAsync("Skip pre-migration script since there is no pending model changes");
				return 0;
			} else {
				var directory = FindTargetVersionScriptLocation(parameters.Directory.FullName);
				await this.Writer.WriteLineAsync($"Found deployment folder: {directory}");
				var info = new DirectoryInfo(directory);
				if (info.Exists) {
					foreach (var file in info.GetFiles("*.sql", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name)) {
						await this.Writer.WriteLineAsync(file.Name);
						await this.executeScriptFile.ExecuteAsync(this.session.DbContext, file.FullName, cancellationToken);
					}
				}
				return 0;
			}
		}
	}
}