using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Sample.Models {
	public class Contact {
		public int Id { get; set; }

		[MaxLength(My.NameLength)]
		public string Name { get; set; } = string.Empty;

		public DateTime CreatedUtc { get; set; }

		public List<Address> Addresses { get; set; } = new List<Address>();

		public ContactProperty? Property { get; }
	}

	public class ContactEntityMap : EntityMap<Contact> {
		public override void Map(EntityTypeBuilder<Contact> builder) {
			base.Map(builder);
			builder.HasKey(x => x.Id);
			builder.Property(x => x.CreatedUtc).UtcDateTimeProperty();
			builder.Property(x => x.Property).HasImmutableJsonProperty();
			builder.HasMany(x => x.Addresses).WithOne(x => x.Contact).HasForeignKey(x => x.ContactId);
		}
	}
}