using System;
using System.Collections.Generic;

namespace Albatross.EFCore {
	/// <summary>
	/// Marks an entity as eligible for change auditing by
	/// <see cref="ChangeAuditInterceptor{TChangeEntity, TEntityId, TActorId}"/>.
	/// Implement on entities whose modifications and deletions should produce audit records.
	/// </summary>
	public interface IAuditable<out TEntityId> {
		TEntityId Id { get; }
		IReadOnlyCollection<string> MaskedProperties => Array.Empty<string>();
		IReadOnlyCollection<string> IgnoredProperties => Array.Empty<string>();
	}
}