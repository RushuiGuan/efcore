using System;

namespace Albatross.EFCore {
	public enum ChangeType {
		Added = 1,
		Deleted = 2,
		Modified = 4,
		None = 0,
		All = Added | Deleted | Modified,
	}

	public class Change<TEntityId, TActorId> {
		public required TEntityId EntityId { get; init; }
		public required TActorId ActorId { get; init; }
		public required DateTime UtcTimeStamp { get; init; }
		public required ChangeType ChangeType { get; init; }
		public required string EntityType { get; init; }
		public string? Json { get; init; }
	}
}