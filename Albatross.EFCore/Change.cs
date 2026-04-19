using System;

namespace Albatross.EFCore {
	/// <summary>
	/// Flags enum describing which EF Core entity state transitions are tracked or reported.
	/// <see cref="All"/> is a convenience combination of <see cref="Added"/>, <see cref="Modified"/>,
	/// and <see cref="Deleted"/>.
	/// </summary>
	[Flags]
	public enum ChangeType {
		Added = 1,
		Deleted = 2,
		Modified = 4,
		None = 0,
		All = Added | Deleted | Modified,
	}

	/// <summary>
	/// Immutable audit-trail record capturing who changed which entity, when, and how.
	/// Persisted to the database by <see cref="ChangeAuditInterceptor{TChangeEntity, TEntityId, TActorId}"/>.
	/// </summary>
	public class Change<TEntityId, TActorId> {
		public required TEntityId EntityId { get; init; }
		public required TActorId ActorId { get; init; }
		public required DateTime UtcTimeStamp { get; init; }
		public required ChangeType ChangeType { get; init; }
		public required string EntityType { get; init; }

		/// <summary>
		/// JSON snapshot of the changed properties. For <see cref="ChangeType.Deleted"/> records, contains
		/// original values only; for <see cref="ChangeType.Modified"/>, contains both original and current
		/// values per property. Null when there are no property-level changes to record.
		/// </summary>
		public string? Json { get; init; }
	}
}