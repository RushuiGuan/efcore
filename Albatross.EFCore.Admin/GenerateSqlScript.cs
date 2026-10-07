using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.CommandLine.Outputs;
using Microsoft.EntityFrameworkCore;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class GenerateSqlScriptParams {
		[UseOption<OutputFileOption>]
		public FileInfo? OutputFile { get; init; }
	}

	public class GenerateSqlScript<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly T session;
		private readonly ParseResult parseResult;
		private readonly GenerateSqlScriptParams parameters;

		public GenerateSqlScript(T session, ParseResult parseResult, GenerateSqlScriptParams parameters) {
			this.session = session;
			this.parseResult = parseResult;
			this.parameters = parameters;
		}
		public Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string script = session.DbContext.Database.GenerateCreateScript();
			if (parameters.OutputFile != null) {
				if (parameters.OutputFile.Directory != null) {
					Directory.CreateDirectory(parameters.OutputFile.Directory.FullName);
				}
				File.WriteAllText(parameters.OutputFile.FullName, script);
				parseResult.PrintSuccess($"Sql script written to {parameters.OutputFile.FullName}");
			} else {
				System.Console.WriteLine(script);
			}
			return Task.FromResult(0);
		}
	}
}