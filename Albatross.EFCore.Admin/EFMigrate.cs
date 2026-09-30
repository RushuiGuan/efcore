using Albatross.CommandLine;
using Albatross.CommandLine.Outputs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class EFMigrationParams {
	}

	public class EFMigrate<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly T session;
		private readonly ILogger<EFMigrate<T>> logger;
		private readonly ParseResult result;

		public EFMigrate(T session, ILogger<EFMigrate<T>> logger, ParseResult result) {
			this.session = session;
			this.logger = logger;
			this.result = result;
		}

		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting {provider} migration", session.DbContext.Database.ProviderName);
			await session.DbContext.Database.MigrateAsync(cancellationToken);
			result.PrintSuccess("Migration succeeded");
			return 0;
		}
	}
}