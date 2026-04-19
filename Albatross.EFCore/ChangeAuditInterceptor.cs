using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// EF Core save interceptor that writes a <typeparamref name="TChangeEntity"/> audit record for every
	/// <see cref="IAuditable{TEntityId}"/> entity that is modified or deleted in the same transaction.
	/// Added entities are intentionally excluded — only modifications and deletions are audited.
	/// </summary>
	/// <typeparam name="TChangeEntity">The audit-log entity type; must derive from <see cref="Change{TEntityId, TActorId}"/>.</typeparam>
	/// <typeparam name="TEntityId">Primary-key type of the audited entities.</typeparam>
	/// <typeparam name="TActorId">Type used to identify who performed the change (e.g. <c>string</c> for a username).</typeparam>
	/// <remarks>
	/// Register the interceptor as a scoped service and wire it into <see cref="DbContextOptions"/>:
	/// <code>
	/// services.AddScoped&lt;ChangeAuditInterceptor&lt;AuditLog, int, string&gt;&gt;();
	/// services.AddDbContext&lt;MyDbSession&gt;((sp, options) =&gt; {
	///     options.AddInterceptors(sp.GetRequiredService&lt;ChangeAuditInterceptor&lt;AuditLog, int, string&gt;&gt;());
	/// });
	/// </code>
	/// Requires <see cref="IGetCurrentActorId{T}"/> and <see cref="TimeProvider"/> in DI.
	/// </remarks>
	public class ChangeAuditInterceptor<TChangeEntity, TEntityId, TActorId> : SaveChangesInterceptor where TChangeEntity : Change<TEntityId, TActorId>, new() {
		private readonly IGetCurrentActorId<TActorId> getCurrentActorId;
		private readonly TimeProvider timeProvider;

		public ChangeAuditInterceptor(IGetCurrentActorId<TActorId> getCurrentActorId, TimeProvider timeProvider) {
			this.getCurrentActorId = getCurrentActorId;
			this.timeProvider = timeProvider;
		}

		/// <summary>Captures the before/after values of a single property in the JSON snapshot.</summary>
		public class ChangedProperty {
			public object? Original { get; init; }
			public object? Current { get; init; }
		}

		string? GetJson(EntityEntry entry) {
			var dict = new Dictionary<string, ChangedProperty>();
			foreach (var property in entry.Properties) {
				if (property.Metadata.IsConcurrencyToken) {
					continue;
				}
				if (entry.State == EntityState.Deleted) {
					dict[property.Metadata.Name] = new ChangedProperty {
						Original = property.OriginalValue,
					};
				} else if (entry.State == EntityState.Modified) {
					if (property.IsModified) {
						dict[property.Metadata.Name] = new ChangedProperty {
							Original = property.OriginalValue,
							Current = property.CurrentValue,
						};
					}
				}
			}
			if (dict.Count > 0) {
				return JsonSerializer.Serialize(dict);
			} else {
				return null;
			}
		}

		void CollectChanges(DbContext context) {
			var actor = getCurrentActorId.Get();
			var now = timeProvider.GetUtcNow().UtcDateTime;
			var changes = new List<TChangeEntity>();
			// Safe to enumerate directly (no mutation during loop). Avoid ToList() for perf reasons.
			foreach (var entry in context.ChangeTracker.Entries()) {
				if (entry.Entity is Change<TEntityId, TActorId>) {
					continue;
				}
				if (entry.Entity is IAuditable<TEntityId> auditable) {
					ChangeType changeType;
					switch (entry.State) {
						case EntityState.Modified:
							changeType = ChangeType.Modified;
							break;
						case EntityState.Deleted:
							changeType = ChangeType.Deleted;
							break;
						default:
							continue;
					}
					var json = GetJson(entry);
					if (json != null) {
						changes.Add(new TChangeEntity() {
							EntityId = auditable.Id,
							ActorId = actor,
							UtcTimeStamp = now,
							ChangeType = changeType,
							EntityType = entry.Metadata.ClrType.Name,
							Json = json,
						});
					}
				}
			}
			if (changes.Count > 0) {
				context.AddRange(changes);
			}
		}

		public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
			if (eventData.Context is not null) {
				CollectChanges(eventData.Context);
			}
			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}
	}
}