using Albatross.Serialization.Json;
using System;
using System.Text.Json;

namespace Albatross.EFCore {
	public class EFCoreJsonOption : IJsonSettings {
		public JsonSerializerOptions Value { get; }
		public static IJsonSettings Instance => lazy.Value;

		public EFCoreJsonOption() {
			Value = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
				Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
			};
		}
		readonly static Lazy<EFCoreJsonOption> lazy = new Lazy<EFCoreJsonOption>(() => new EFCoreJsonOption());
	}
}