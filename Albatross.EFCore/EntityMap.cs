using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Albatross.EFCore {
	/// <summary>
	/// Base implementation of <see cref="IEntityMap{T}"/> that bridges entity configuration
	/// into the <see cref="IBuildEntityModel"/> pipeline.
	/// Override <see cref="Map"/> to apply Fluent API configuration for <typeparamref name="T"/>.
	/// </summary>
	public abstract class EntityMap<T> : IBuildEntityModel, IEntityMap<T> where T : class {
		public virtual void Map(EntityTypeBuilder<T> builder) {
			// builder.ToTable<T>(TableName, this.BuildTable);
		}
		public void Build(ModelBuilder builder) {
			var entityTypeBuilder = builder.Entity<T>();
			Map(entityTypeBuilder);
		}
	}
}
