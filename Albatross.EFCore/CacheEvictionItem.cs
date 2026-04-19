using Microsoft.EntityFrameworkCore;
using System;

namespace Albatross.EFCore {
	public record CacheEvictionItem(Type EntityType, object Entity, EntityState State);
}