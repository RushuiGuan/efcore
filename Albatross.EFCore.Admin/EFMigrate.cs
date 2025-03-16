using Albatross.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public class EFMigrationOptions { }

	public class EFMigrate<T> : BaseHandler<EFMigrationOptions> where T:IDbSession{
		private readonly T session;

		public EFMigrate(T session, IOptions<EFMigrationOptions> options) : base(options) {
			this.session = session;
		}
		
		public override async Task<int> InvokeAsync(InvocationContext context) {
			await session.DbContext.Database.MigrateAsync();
			return 0;
		}
	}
}