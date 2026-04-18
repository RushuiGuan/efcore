using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore.AutoCacheEviction {
	/// <summary>
	/// This interceptor should be registered as a scoped service. It collects changes of type TEntity during SaveChanges and generates a report after changes are saved successfully.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class AutoCacheEvictionInterceptor  : SaveChangesInterceptor {
		public required Func<List<ChangeReport<TEntity>>, ValueTask> OnReportGenerated { get; init; }
		private readonly List<ChangeReport<TEntity>> changes = new();

		void CollectChanges(DbContext context) {
			this.changes.Clear();
			foreach (var entry in context.ChangeTracker.Entries()) {
				if (entry.Entity is TEntity entity) {
					if (entry.State == EntityState.Modified && (ChangeType & ChangeType.Modified) != 0) {
						changes.AddRange(entry.Properties
							.Where(args => !ShouldSkip(args.Metadata) && args.IsModified)
							.Select(args => new ChangeReport<TEntity>(entity, ChangeType.Modified, args.Metadata.Name) {
								OldValue = args.OriginalValue,
								NewValue = args.CurrentValue,
							}));
					} else if (entry.State == EntityState.Deleted && (ChangeType & ChangeType.Deleted) != 0) {
						changes.AddRange(entry.Properties
							.Where(args => !ShouldSkip(args.Metadata))
							.Select(args => new ChangeReport<TEntity>(entity, ChangeType.Deleted, args.Metadata.Name) {
								OldValue = args.OriginalValue,
								NewValue = null,
							}));
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
					await OnReportGenerated(this.changes);
				} catch (Exception err) {
					logger.LogError(err, "Error occurred while generating change report");
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