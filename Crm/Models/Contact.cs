using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Crm.Models {
	public class Contact {
		public required Guid Id { get; init; }

		public required Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;

		[MaxLength(Constants.NameLength)]
		public required string Name { get; set; }

		public ICollection<Address> Addresses { get; init; } = [];
	}

	public class ContactEntityMap : EntityMap<Contact> {
		public override void Map(EntityTypeBuilder<Contact> builder) {
			base.Map(builder);
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.HasMany(x => x.Addresses).WithOne(x => x.Contact).HasForeignKey(x => x.ContactId);
		}
	}
}