using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// EF Core save interceptor that captures per-property changes for <typeparamref name="TEntity"/>
	/// and invokes <see cref="OnReportGenerated"/> after each successful save.
	/// Register as a scoped service so each request gets its own isolated change list.
	/// </summary>
	public class ChangeReportInterceptor<TEntity> : SaveChangesInterceptor where TEntity : class {
		private readonly ILogger<ChangeReportInterceptor<TEntity>> logger;

		public ChangeReportInterceptor(ILogger<ChangeReportInterceptor<TEntity>> logger) {
			this.logger = logger;
		}

		/// <summary>
		/// Controls which entity state transitions are tracked. Defaults to <see cref="ChangeType.All"/>.
		/// </summary>
		public ChangeType ChangeType { get; init; } = ChangeType.All;

		/// <summary>
		/// Predicate applied to each EF Core property to exclude it from reporting.
		/// By default, concurrency tokens are skipped.
		/// </summary>
		public Func<IProperty, bool> ShouldSkip { get; init; } = property => {
			if (property.IsConcurrencyToken) {
				return true;
			} else {
				return false;
			}
		};

		/// <summary>
		/// Required callback invoked with the full change list after each successful save.
		/// Only called when at least one change was tracked; errors are caught and logged
		/// without rolling back the save.
		/// </summary>
		public required Func<List<ChangeReport<TEntity>>, ValueTask> OnReportGenerated { get; init; }
		private readonly List<ChangeReport<TEntity>> changes = new();

		void CollectChanges(DbContext context) {
			this.changes.Clear();
			foreach (var entry in context.ChangeTracker.Entries()) {
				if (entry.Entity is TEntity entity) {
					if (entry.State == EntityState.Added && (ChangeType & ChangeType.Added) != 0) {
						changes.AddRange(entry.Properties
							.Where(args => !ShouldSkip(args.Metadata))
							.Select(args => new ChangeReport<TEntity>(entity, ChangeType.Added, args.Metadata.Name) {
								OldValue = null,
								NewValue = args.CurrentValue,
							}));
					} else if (entry.State == EntityState.Modified && (ChangeType & ChangeType.Modified) != 0) {
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