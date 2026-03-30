using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Crm.Models {
	public interface ICrmDbSession : IDbSession {
	}

	public class CrmDbSession : DbSessionWithEventHandlers, ICrmDbSession {
		public CrmDbSession(DbContextOptions option, IDbEventSessionProvider eventSessionProvider)
			: base(option, eventSessionProvider) {
		}

		protected CrmDbSession(DbContextOptions option) : base(option) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.HasDefaultSchema(My.Schema.Sample);
			modelBuilder.BuildEntityModels();
		}
	}
}