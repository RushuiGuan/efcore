using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	public interface ICacheEvictionService<T> {
		IEnumerable<T> GetCacheKeys(CacheEvictionItem cacheItem);
		ValueTask Evict(IEnumerable<T> items, CancellationToken cancellationToken);
	}
	/// <summary>
	/// EF Core save interceptor that triggers cache eviction for modified or deleted entities.
	/// Register as a scoped service; configure <see cref="GetCacheKeys"/> and <see cref="Evict"/>
	/// before the first save in the scope, or provide an <see cref="ICacheEvictionService{T}"/> to wire them automatically.
	/// </summary>
	public class CacheEvictionInterceptor<T> : SaveChangesInterceptor {
		public CacheEvictionInterceptor(ILogger<CacheEvictionInterceptor<T>> logger) {
			this.logger = logger;
		}

		[SetsRequiredMembers]
		public CacheEvictionInterceptor(ILogger<CacheEvictionInterceptor<T>> logger, ICacheEvictionService<T> cacheEvictionService) {
			this.logger = logger;
			this.GetCacheKeys = cacheEvictionService.GetCacheKeys;
			this.Evict = cacheEvictionService.Evict;
		}

		private readonly ILogger<CacheEvictionInterceptor<T>> logger;

		/// <summary>
		/// Predicate that determines whether a changed entity's cache entry should be evicted.
		/// Evaluated before the save completes so you can filter by entity type or state.
		/// </summary>
		public required Func<CacheEvictionItem, IEnumerable<T>> GetCacheKeys { get; init; }

		/// <summary>
		/// Required callback invoked after a successful save with all items that passed <see cref="GetCacheKeys"/>.
		/// Errors are caught and logged; cache failures do not roll back the save.
		/// </summary>
		public required Func<IEnumerable<T>, CancellationToken, ValueTask> Evict { get; init; }

		private readonly HashSet<T> cacheKeys = new();

		void CollectChanges(DbContext context) {
			this.cacheKeys.Clear();
			foreach (var entry in context.ChangeTracker.Entries()) {
				if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted) {
					var item = new CacheEvictionItem(entry);
					foreach (var key in GetCacheKeys(item)) {
						cacheKeys.Add(key);
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
			if (this.cacheKeys.Count > 0) {
				try {
					await Evict(this.cacheKeys, cancellationToken);
				} catch (Exception err) {
					logger.LogError(err, "Error occurred while evicting cache");
				} finally {
					this.cacheKeys.Clear();
				}
			}
			return await base.SavedChangesAsync(eventData, result, cancellationToken);
		}

		public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default) {
			this.cacheKeys.Clear();
			return base.SaveChangesFailedAsync(eventData, cancellationToken);
		}
	}
}