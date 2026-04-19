using System;
using System.Text.Json;

namespace Albatross.EFCore {
	/// <summary>
	/// Shared <see cref="JsonSerializerOptions"/> used for all EF Core JSON column conversions:
	/// camel-case property names, enums serialized as strings, and default-value properties omitted.
	/// </summary>
	public class EFCoreJsonOption {
		public static JsonSerializerOptions Value => lazy.Value;

		readonly static Lazy<JsonSerializerOptions> lazy = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
			Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
		});
	}
}
