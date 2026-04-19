using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Models {
	/// <summary>
	/// Single audit table for all Guid-keyed entities in the CRM bounded context.
	/// The EntityType column distinguishes records from Company, Contact, Address, etc.
	/// </summary>
	public class Audit : Change<Guid, Guid> {
		public int Id { get; init; }
	}

	public class AuditEntityMap : EntityMap<Audit> {
		public override void Map(EntityTypeBuilder<Audit> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
			builder.Property(x => x.EntityType).HasMaxLength(256);
		}
	}
}