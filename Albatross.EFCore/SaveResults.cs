using System;
using System.Diagnostics.CodeAnalysis;

namespace Albatross.EFCore {
	/// <summary>
	/// The outcome of a <see cref="IRepository.SaveChangesAsync"/> call, including structured
	/// constraint-violation flags so callers can respond without parsing exception messages.
	/// </summary>
	/// <remarks>
	/// Callers (controllers or command handlers) handle <see cref="SaveResults"/> at the application
	/// boundary — after all service calls in the unit of work are complete:
	/// <code>
	/// var result = await repository.SaveChangesAsync(throwException: false, cancellationToken);
	/// if (!result.Success) {
	///     if (result.NameConflict) return Conflict("Name already exists");
	///     if (result.ForeignKeyConflict) return UnprocessableEntity("Referenced entity not found");
	///     throw result.Error!;
	/// }
	/// </code>
	/// </remarks>
	public record SaveResults {
		[MemberNotNullWhen(false, nameof(Error))]
		public bool Success => Error == null;

		/// <summary>True when the save failed due to a unique-key constraint violation.</summary>
		public bool NameConflict { get; init; }

		/// <summary>True when the save failed due to a foreign-key constraint violation.</summary>
		public bool ForeignKeyConflict { get; init; }
		
		public bool ConcurrencyConflict { get; init; }

		/// <summary>
		/// The exception that caused the save to fail — the inner exception if one exists, otherwise the
		/// original. Null when <see cref="Success"/> is true.
		/// </summary>
		public Exception? Error { get; init; }
	}
}