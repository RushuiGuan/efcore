﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace Albatross.EFCore {
	public static class Extensions {
		/// <summary>
		/// This method maps a text column to a Json property.  It will create a non unicode column with max length. varchar(max).
		/// It is recommended to use a sql utf8 collation. 
		/// *** IMPROTANT *** The json property has to be an immutable object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="builder"></param>
		/// <returns></returns>
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
				} catch {
					return default;
				}
			}
		}
		
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

		public static PropertyBuilder<DateTime> UtcDateTimeProperty(this PropertyBuilder<DateTime> builder) {
			builder.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
			return builder;
		}

		public static PropertyBuilder<DateTime?> UtcDateTimeProperty(this PropertyBuilder<DateTime?> builder) {
			builder.HasConversion(value => value, item => item.HasValue ? DateTime.SpecifyKind(item.Value, DateTimeKind.Utc) : null);
			return builder;
		}

		public static IServiceCollection AddDbSessionEvents(this IServiceCollection services) {
			services.TryAddScoped<IDbEventSession, DbEventSession>();
			services.TryAddSingleton<IDbEventSessionProvider, DbEventSessionProvider>();
			return services;
		}
	}
}