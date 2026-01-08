using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class GenerateSqlScriptParams {
		[UseOption<OutputFileOption>]
		public FileInfo? OutputFile { get; init; }

		[UseOption<OutputFileOption>(Description = "Drop table scripts", UseCustomName = true)]
		public FileInfo? DropScript { get; set; }
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
		public TextWriter Writer => parseResult.InvocationConfiguration.Output;
		public Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string script = session.GetCreateScript();
			using (var reader = new StringReader(script)) {
				string content = reader.ReadToEnd();
				this.Writer.WriteLine(content);
				if (parameters.OutputFile != null) {
					using (var file = new StreamWriter(parameters.OutputFile.FullName)) {
						file.WriteLine(content);
					}
				}
			}
			if (parameters.DropScript != null) {
				List<string> tables = new List<string>();
				Regex regex = new Regex(@"^CREATE TABLE (\[\w+\]\.\[\w+\]) \($", RegexOptions.IgnoreCase | RegexOptions.Singleline);
				using (StringReader reader = new StringReader(script)) {
					for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
						if (!string.IsNullOrEmpty(line)) {
							var match = regex.Match(line);
							if (match.Success) {
								tables.Add(match.Groups[1].Value);
							}
						}
					}
				}

				using (var writer = new StreamWriter(parameters.DropScript.FullName)) {
					var schema = session.DbContext.Model.GetDefaultSchema() ?? "dbo";
					tables.Reverse();
					foreach (var table in tables) {
						if (table.StartsWith($"[{schema}]")) {
							writer.Write("drop table ");
							writer.WriteLine(table);
						}
					}
					writer.WriteLine($"drop table [{schema}].[__EFMigrationsHistory]");
				}
			}
			return Task.FromResult(0);
		}
	}
}