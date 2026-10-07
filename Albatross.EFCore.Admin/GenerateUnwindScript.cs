using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.CommandLine.Outputs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class GenerateUnwindScriptParams {
		[UseOption<OutputFileOption>]
		public FileInfo? OutputFile { get; init; }

		[Option(Description = "Generate an idempotent script that checks the migration history before reverting each migration, so it can run against a database at any migration level")]
		public bool Idempotent { get; init; }
	}

	public class GenerateUnwindScript<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly T session;
		private readonly ParseResult parseResult;
		private readonly GenerateUnwindScriptParams parameters;

		public GenerateUnwindScript(T session, ParseResult parseResult, GenerateUnwindScriptParams parameters) {
			this.session = session;
			this.parseResult = parseResult;
			this.parameters = parameters;
		}

		public Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var latest = session.DbContext.Database.GetMigrations().LastOrDefault();
			if (latest == null) {
				return Task.FromResult(parseResult.PrintError("No migrations found"));
			}
			var migrator = session.DbContext.GetService<IMigrator>();
			var options = parameters.Idempotent ? MigrationsSqlGenerationOptions.Idempotent : MigrationsSqlGenerationOptions.Default;
			string script = migrator.GenerateScript(latest, Migration.InitialDatabase, options);
			if (parameters.OutputFile != null) {
				if (parameters.OutputFile.Directory != null) {
					Directory.CreateDirectory(parameters.OutputFile.Directory.FullName);
				}
				File.WriteAllText(parameters.OutputFile.FullName, script);
				parseResult.PrintSuccess($"Unwind script from {latest} written to {parameters.OutputFile.FullName}");
			} else {
				System.Console.WriteLine(script);
			}
			return Task.FromResult(0);
		}
	}
}
