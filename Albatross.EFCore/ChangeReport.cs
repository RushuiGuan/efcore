namespace Albatross.EFCore {
	public class ChangeReport<T> where T : class {
		public ChangeReport(T entity, ChangeType type, string property) {
			Entity = entity;
			this.Type = type;
			Property = property;
		}

		public T Entity { get; }
		public ChangeType Type { get; }
		public string Property { get; }
		public object? OldValue { get; init; }
		public object? NewValue { get; init; }
	}
}