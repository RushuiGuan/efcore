using Albatross.Exceptions;
using System;
using System.Collections.Generic;

namespace Albatross.EFCore {
	/// <summary>
	/// Strongly-typed variant of <see cref="NotFoundException"/> that derives the entity name
	/// from <typeparamref name="T"/>, eliminating magic strings at the call site.
	/// </summary>
	public class NotFoundException<T> : NotFoundException {
		public NotFoundException(params IEnumerable<string> keys) : base($"{typeof(T).Name} ({string.Join(", ", keys)}) is not found") { }
		public NotFoundException(int id) : base($"{typeof(T).Name} ({id}) is not found") { }
		public NotFoundException(Guid id) : base($"{typeof(T).Name} ({id}) is not found") { }
	}
}