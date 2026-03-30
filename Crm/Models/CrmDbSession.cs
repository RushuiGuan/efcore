using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Crm.Models {
	public interface ICrmDbSession : IDbSession {
	}

	public class CrmDbSession : DbSessionWithEventHandlers, ICrmDbSession {
		public CrmDbSession(DbContextOptions option) : base(option) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.HasDefaultSchema(Constants.Schema);
			modelBuilder.BuildEntityModels();
		}
	}
}