using Albatross.CommandLine;
using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class GenerateSqlScriptOptions {
		[Option("o", "--output-file", Description = "Set the output file")]
		public string? Out { get; set; }

		[Option("d", Description = "Drop table scripts")]
		public string? DropScript { get; set; }
	}

	public class GenerateSqlScript<T> : BaseHandler<GenerateSqlScriptOptions> where T:IDbSession{
		private readonly T session;

		public GenerateSqlScript(T session, IOptions<GenerateSqlScriptOptions> options) : base(options) {
			this.session = session;
		}
		public override Task<int> InvokeAsync(InvocationContext context) {
			string script = session.GetCreateScript();
			using (var reader = new StringReader(script)) {
				string content = reader.ReadToEnd();
				this.writer.WriteLine(content);
				if (!string.IsNullOrEmpty(options.Out)) {
					using (var file = new StreamWriter(options.Out)) {
						file.WriteLine(content);
					}
				}
			}
			if (!string.IsNullOrEmpty(options.DropScript)) {
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

				using (var writer = new StreamWriter(options.DropScript)) {
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