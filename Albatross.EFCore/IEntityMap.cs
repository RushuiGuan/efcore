using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Albatross.EFCore {
	/// <summary>
	/// Convention for configuring the EF Core entity model for <typeparamref name="T"/> via Fluent API.
	/// Implement and register through the code-generated <c>BuildEntityModels</c> extension on
	/// <see cref="Microsoft.EntityFrameworkCore.ModelBuilder"/>.
	/// </summary>
	/// <remarks>
	/// By convention, co-locate the entity class and its <see cref="IEntityMap{T}"/> implementation
	/// in the same file. Define keys, indexes, and relationships here — never use
	/// <c>[ForeignKey]</c> or <c>[InverseProperty]</c> data annotations for relationship configuration.
	/// </remarks>
	public interface IEntityMap<T> where T : class {
		void Map(EntityTypeBuilder<T> builder);
	}
}
