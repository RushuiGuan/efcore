using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace Albatross.EFCore {
	public static class Extensions {
		/// <summary>
		/// Maps a complex property to a <c>varchar(max)</c> JSON column using the shared
		/// <see cref="EFCoreJsonOption"/> serializer settings.
		/// </summary>
		/// <remarks>
		/// The property type must be immutable (or implement value-equality) because EF Core uses
		/// reference equality for change detection on value-converted properties. Mutating the object
		/// in place will not be detected as a change — assign a new instance instead.
		/// A UTF-8 database collation is recommended for correct JSON storage without Unicode overhead.
		/// </remarks>
		public static PropertyBuilder<T?> HasImmutableJsonProperty<T>(this PropertyBuilder<T?> builder) {
			builder.IsRequired(false).IsUnicode(false).HasConversion(new ValueConverter<T?, string?>(
					args => SaveNullableJsonData(args),
					args => GetNullableJsonData<T>(args)),
				new ValueComparer<T?>(
					(left, right) => EqualityComparer<T?>.Default.Equals(left, right),
					obj => obj == null ? 0 : obj.GetHashCode(),
					obj => obj == null ? default : obj
				));
			return builder;
		}

		static string? SaveNullableJsonData<T>(T? data) {
			if (data == null) {
				return null;
			} else {
				return JsonSerializer.Serialize(data, EFCoreJsonOption.Value);
			}
		}

		static T? GetNullableJsonData<T>(string? text) {
			if (string.IsNullOrEmpty(text)) {
				return default;
			} else {
				try {
					return JsonSerializer.Deserialize<T>(text, EFCoreJsonOption.Value);
				} catch (JsonException) {
					return default;
				} catch (NotSupportedException) {
					return default;
				}
			}
		}

		/// <summary>
		/// Maps a <see cref="List{TProperty}"/> to a <c>varchar(max)</c> JSON column.
		/// An empty list is stored as <c>NULL</c>; a <c>NULL</c> or empty column is read back as an empty list.
		/// </summary>
		public static PropertyBuilder<List<TProperty>> HasJsonCollectionProperty<TProperty>(this PropertyBuilder<List<TProperty>> builder) {
			builder.IsRequired(false).IsUnicode(false).HasConversion(new ValueConverter<List<TProperty>, string?>(
					args => args.Any() ? JsonSerializer.Serialize(args, EFCoreJsonOption.Value) : null,
					args => string.IsNullOrEmpty(args) ? new List<TProperty>() : JsonSerializer.Deserialize<List<TProperty>>(args, EFCoreJsonOption.Value) ?? new List<TProperty>()),
				new ValueComparer<List<TProperty>>(
					(left, right) => left == right || left != null && right != null && left.SequenceEqual(right),
					obj => obj.Aggregate(0, (a, b) => HashCode.Combine(a, b == null ? 0 : b.GetHashCode())),
					obj => new List<TProperty>(obj)));
			return builder;
		}

		public static void ValidateByDataAnnotations(this object entity) {
			Validator.ValidateObject(entity, new ValidationContext(entity), true);
		}

		/// <summary>
		/// Ensures a <see cref="DateTime"/> column is read back with <see cref="DateTimeKind.Utc"/> set.
		/// Without this, EF Core returns <see cref="DateTimeKind.Unspecified"/>, which causes incorrect
		/// behavior in time-zone-aware comparisons.
		/// </summary>
		public static PropertyBuilder<DateTime> UtcDateTimeProperty(this PropertyBuilder<DateTime> builder) {
			builder.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
			return builder;
		}

		/// <inheritdoc cref="UtcDateTimeProperty(PropertyBuilder{DateTime})"/>
		public static PropertyBuilder<DateTime?> UtcDateTimeProperty(this PropertyBuilder<DateTime?> builder) {
			builder.HasConversion(value => value, item => item.HasValue ? DateTime.SpecifyKind(item.Value, DateTimeKind.Utc) : null);
			return builder;
		}
	}
}