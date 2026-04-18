namespace Albatross.EFCore {
	public interface IAuditable<out TEntityId> {
		TEntityId Id { get; }
	}
}