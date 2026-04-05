using System;
using System.Collections.Generic;

namespace Albatross.EFCore {
	public class NotFoundException : Exception{
		// message look like "Resource (default, cooper) is not found" 
		public NotFoundException(string entity, params IEnumerable<string> keys) : base($"{entity} ({string.Join(", ", keys)}) is not found") { }
		public NotFoundException(string entity, int id) : base($"{entity} ({id}) is not found") { }
		public NotFoundException(string entity, Guid id) : base($"{entity} ({id}) is not found") { }
	}
	
	public class NotFoundException<T> : NotFoundException {
		public NotFoundException(params IEnumerable<string> keys) : base(typeof(T).Name, keys) { }
		public NotFoundException(int id) : base(typeof(T).Name, id) { }
		public NotFoundException(Guid id) : base(typeof(T).Name, id) { }
	}
}