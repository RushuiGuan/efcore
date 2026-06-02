using System;

namespace Albatross.EFCore {
	public class PropertyNotLoadedException : Exception {
		public PropertyNotLoadedException(string entity, string property) : base($"Navigation property '{property}' on '{entity}' is not loaded") { 
		}
	}
}
