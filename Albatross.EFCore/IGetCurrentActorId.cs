namespace Albatross.EFCore {
	/// <summary>
	/// Resolves the identity of the actor performing the current operation, used by
	/// <see cref="ChangeAuditInterceptor{TChangeEntity, TEntityId, TActorId}"/> to record who made each change.
	/// Register a scoped implementation that reads from the ambient context (e.g. the HTTP request's user claim).
	/// </summary>
	public interface IGetCurrentActorId<out T> {
		T Get();
	}
}
