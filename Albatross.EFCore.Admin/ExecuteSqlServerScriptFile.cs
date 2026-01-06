using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class ExecuteSqlServerScriptFile : IExecuteScriptFile {
		private readonly ILogger<ExecuteSqlServerScriptFile> logger;

		public ExecuteSqlServerScriptFile(ILogger<ExecuteSqlServerScriptFile> logger) {
			this.logger = logger;
		}

		static Regex goRegex = new Regex(@"^\s*go\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
		
		async Task Execute(DbContext context, StringBuilder sb, CancellationToken cancellationToken) {
			var text = sb.ToString().Trim();
			if (!string.IsNullOrWhiteSpace(text)) {
				logger.LogInformation("Executing: {query}", text);
				await context.Database.ExecuteSqlRawAsync(text,cancellationToken);
			}
			sb.Length = 0;
		}

		public async Task ExecuteAsync(DbContext context, string filename, CancellationToken cancellationToken) {
			var file = new FileInfo(filename);
			if (file.Exists) {
				logger.LogInformation($"Executing migration file: {file}", file.Name);
				var stream = file.OpenRead();
				var reader = new StreamReader(stream);
				var sb = new StringBuilder();
				for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
					if (goRegex.IsMatch(line)) {
						await Execute(context, sb, cancellationToken);
					} else {
						sb.AppendLine(line);
					}
				}
				await Execute(context, sb, cancellationToken); 
			}
		}
	}
}