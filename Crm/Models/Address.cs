using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Crm.Models {
	public class Address {
		public required Guid Id { get; init; }

		public required Guid ContactId { get; set; }
		public Contact Contact { get; set; } = null!;

		[MaxLength(Constants.TitleLength)]
		public string? Line1 { get; set; }

		[MaxLength(Constants.TitleLength)]
		public string? Line2 { get; set; }

		[MaxLength(Constants.TitleLength)]
		public string? City { get; set; }

		[MaxLength(Constants.TitleLength)]
		public string? State { get; set; }

		[MaxLength(Constants.TitleLength)]
		public string? PostalCode { get; set; }
	}

	public class AddressEntityMap : EntityMap<Address> {
		public override void Map(EntityTypeBuilder<Address> builder) {
			base.Map(builder);
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
		}
	}
}