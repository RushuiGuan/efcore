using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class ExecuteScriptFile : IExecuteScriptFile {
		private readonly ILogger<ExecuteScriptFile> logger;

		public ExecuteScriptFile(ILogger<ExecuteScriptFile> logger) {
			this.logger = logger;
		}

		public async Task ExecuteAsync(DbContext context, string filename) {
			logger.LogInformation("Executing migration script: {name}", filename);
			var script = await File.ReadAllTextAsync(filename);
			await context.Database.ExecuteSqlRawAsync(script);
		}
	}
}