using Albatross.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class EFMigrationParams {
	}

	public class EFMigrate<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly T session;
		private readonly ILogger<EFMigrate<T>> logger;

		public EFMigrate(T session, ILogger<EFMigrate<T>> logger) {
			this.session = session;
			this.logger = logger;
		}

		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting {provider} migration", session.DbContext.Database.ProviderName);
			await session.DbContext.Database.MigrateAsync(cancellationToken);
			return 0;
		}
	}
}