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
			var fileInfo = new FileInfo(filename);
			if (fileInfo.Exists) {
				logger.LogInformation("Executing migration script: {name}", fileInfo.Name);
				var reader = fileInfo.OpenText();
				var script = await reader.ReadToEndAsync();
				if(!string.IsNullOrEmpty(script)) {
					await context.Database.ExecuteSqlRawAsync(script);
				}
			}
		}
	}
}