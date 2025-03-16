using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Albatross.EFCore {
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