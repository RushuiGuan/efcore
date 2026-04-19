using Microsoft.EntityFrameworkCore;
using System;

namespace Albatross.EFCore {
	/// <summary>
	/// Identifies an entity whose cache entry should be evicted after a save, including
	/// its CLR type and EF Core state at the time of the change.
	/// </summary>
	public record CacheEvictionItem(Type EntityType, object Entity, EntityState State);
}
