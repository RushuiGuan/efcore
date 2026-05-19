using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace Albatross.EFCore {
	/// <summary>
	/// Identifies an entity whose cache entry should be evicted after a save, including
	/// its CLR type and EF Core state at the time of the change.
	/// </summary>
	public record CacheEvictionItem(EntityEntry Entry) {
		public Type EntityType => Entry.Metadata.ClrType;
		public object Entity => Entry.Entity;
		public EntityState State => Entry.State;
	}
}