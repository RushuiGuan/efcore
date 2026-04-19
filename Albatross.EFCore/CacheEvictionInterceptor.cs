using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// This interceptor should be registered as a scoped service. It collects changes of type TEntity during SaveChanges and generates a report after changes are saved successfully.
	/// </summary>
	public class CacheEvictionInterceptor : SaveChangesInterceptor {
		public CacheEvictionInterceptor(ILogger<CacheEvictionInterceptor> logger) {
			this.logger = logger;
		}

		private readonly ILogger<CacheEvictionInterceptor> logger;
		public required Func<CacheEvictionItem, bool> ShouldEvict { get; init; }
		public required Func<IEnumerable<CacheEvictionItem>, ValueTask> Evict { get; init; }
		private readonly List<CacheEvictionItem> changes = new();

		void CollectChanges(DbContext context) {
			this.changes.Clear();
			foreach (var entry in context.ChangeTracker.Entries()) {
				if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted) {
					var item = new CacheEvictionItem(entry.Metadata.ClrType, entry.Entity, entry.State);
					if (ShouldEvict(item)) {
						changes.Add(item);
					}
				}
			}
		}

		public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
			if (eventData.Context is not null) {
				CollectChanges(eventData.Context);
			}
			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}

		public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default) {
			if (this.changes.Count > 0) {
				try {
					await Evict(this.changes);
				} catch (Exception err) {
					logger.LogError(err, "Error occurred while evicting cache");
				} finally {
					this.changes.Clear();
				}
			}
			return await base.SavedChangesAsync(eventData, result, cancellationToken);
		}

		public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default) {
			this.changes.Clear();
			return base.SaveChangesFailedAsync(eventData, cancellationToken);
		}
	}
}