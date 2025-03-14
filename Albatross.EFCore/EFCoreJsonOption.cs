using System;
using System.Text.Json;

namespace Albatross.EFCore {
	public class EFCoreJsonOption {
		public static JsonSerializerOptions Value => lazy.Value;

		readonly static Lazy<JsonSerializerOptions> lazy = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
			Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
		});
	}
}