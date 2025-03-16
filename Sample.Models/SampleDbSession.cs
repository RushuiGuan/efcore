using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Sample.Models {
	public interface ISampleDbSession : IDbSession {
	}

	public class SampleDbSession : DbSessionWithEventHandlers, ISampleDbSession {
		public SampleDbSession(DbContextOptions option, IDbEventSessionProvider eventSessionProvider)
			: base(option, eventSessionProvider) {
		}

		protected SampleDbSession(DbContextOptions option) : base(option) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.HasDefaultSchema(My.Schema.Sample);
			modelBuilder.BuildEntityModels();
		}
	}
}