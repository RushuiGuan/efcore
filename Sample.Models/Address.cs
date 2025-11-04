using Albatross.EFCore;
using Albatross.EFCore.Audit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Sample.Models {
	public class Address : ICreatedBy, IModifiedBy, ICreatedUtc, IModifiedUtc {
		public int Id { get; set; }

		[MaxLength(My.TitleLength)]
		public string? Line1 { get; set; }

		[MaxLength(My.TitleLength)]
		public string? Line2 { get; set; }

		[MaxLength(My.TitleLength)]
		public string? City { get; set; }

		[MaxLength(My.TitleLength)]
		public string? State { get; set; }

		[MaxLength(My.TitleLength)]
		public string? PostalCode { get; set; }

		public Contact Contact { get; set; } = null!;
		public int ContactId { get; set; }

		[MaxLength(My.NameLength)]
		public string CreatedBy { get; set; } = string.Empty;

		[MaxLength(My.NameLength)]
		public string ModifiedBy { get; set; } = string.Empty;

		public DateTime CreatedUtc { get; set; }
		public DateTime ModifiedUtc { get; set; }
	}

	public class AddressEntityMap : EntityMap<Address> {
		public override void Map(EntityTypeBuilder<Address> builder) {
			base.Map(builder);
			builder.HasKey(x => x.Id);
			builder.Property(x => x.CreatedUtc).UtcDateTimeProperty();
			builder.Property(x => x.ModifiedUtc).UtcDateTimeProperty();
		}
	}
}