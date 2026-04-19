using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Crm.Models {
	public class Company : IAuditable<Guid> {
		public required Guid Id { get; init; }
		[MaxLength(256)] public required string Name { get; set; }
		public string? Description { get; set; }

		public List<Contact> Contacts { get; init; } = [];
	}

	public class CompanyEntityMap : EntityMap<Company> {
		public override void Map(EntityTypeBuilder<Company> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.HasIndex(x => x.Name).IsUnique();
			builder.HasMany(x => x.Contacts).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId);
		}
	}
}