using System;
using System.Collections.Generic;

namespace Albatross.EFCore {
	/// <summary>
	/// Thrown when a required entity cannot be found, with a formatted message that includes
	/// the entity name and key values (e.g. <c>Company (42) is not found</c>).
	/// </summary>
	/// <remarks>
	/// Throw this from repository methods that look up by primary key — where "not found" is an
	/// error condition. For non-key lookups (e.g. by name), a nullable return is more appropriate
	/// since "not found" may be a valid outcome the caller handles.
	/// </remarks>
	public class NotFoundException : Exception{
		// message look like "Resource (default, cooper) is not found"
		public NotFoundException(string entity, params IEnumerable<string> keys) : base($"{entity} ({string.Join(", ", keys)}) is not found") { }
		public NotFoundException(string entity, int id) : base($"{entity} ({id}) is not found") { }
		public NotFoundException(string entity, Guid id) : base($"{entity} ({id}) is not found") { }
	}

	/// <summary>
	/// Strongly-typed variant of <see cref="NotFoundException"/> that derives the entity name
	/// from <typeparamref name="T"/>, eliminating magic strings at the call site.
	/// </summary>
	public class NotFoundException<T> : NotFoundException {
		public NotFoundException(params IEnumerable<string> keys) : base(typeof(T).Name, keys) { }
		public NotFoundException(int id) : base(typeof(T).Name, id) { }
		public NotFoundException(Guid id) : base(typeof(T).Name, id) { }
	}
}
