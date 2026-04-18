namespace Albatross.EFCore {
	public interface IGetCurrentActorId<out T> {
		T Get();
	}
}