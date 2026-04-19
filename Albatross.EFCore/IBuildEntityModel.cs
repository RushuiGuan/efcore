using Microsoft.EntityFrameworkCore;

namespace Albatross.EFCore {
	/// <summary>
	/// Marker and callback interface for entity model configuration. The code-generated
	/// <c>BuildEntityModels</c> extension on <see cref="ModelBuilder"/> discovers and invokes
	/// all registered implementations during <c>OnModelCreating</c>.
	/// </summary>
	public interface IBuildEntityModel {
		void Build(ModelBuilder builder);
	}

	/// <summary>
	/// Scoped variant of <see cref="IBuildEntityModel"/> that constrains model builders to a specific
	/// <see cref="IDbSession"/> type, enabling multiple sessions in the same assembly to register
	/// their entity configurations independently.
	/// </summary>
	public interface IBuildEntityModel<T> : IBuildEntityModel where T : IDbSession {
	}
}
