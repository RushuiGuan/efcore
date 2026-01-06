using Albatross.CommandLine;
using Microsoft.EntityFrameworkCore;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class EFMigrationParams { }

	public class EFMigrate<T> : BaseHandler<EFMigrationParams> where T:IDbSession{
		private readonly T session;

		public EFMigrate(T session, ParseResult result, EFMigrationParams options) : base(result, options) {
			this.session = session;
		}
		
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await session.DbContext.Database.MigrateAsync(cancellationToken);
			return 0;
		}
	}
}