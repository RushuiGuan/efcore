using Albatross.CommandLine;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class EFMigrationParams {
	}

	public class EFMigrate<T> : IAsyncCommandHandler where T : IDbSession {
		private readonly T session;

		public EFMigrate(T session) {
			this.session = session;
		}

		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await session.DbContext.Database.MigrateAsync(cancellationToken);
			return 0;
		}
	}
}