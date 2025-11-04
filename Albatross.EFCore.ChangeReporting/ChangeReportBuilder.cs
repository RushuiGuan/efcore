using Albatross.Reflection;
using Albatross.Text.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Albatross.EFCore.ChangeReporting {
	public class ChangeReportBuilder<T> where T : class {
		public ChangeType Type { get; set; } = ChangeType.Modified;
		public string? Prefix { get; set; }
		public string? Postfix { get; set; }
		public TableOptions<ChangeReport<T>>? TableOptions { get; set; }
		public List<string> SkippedProperties { get; init; } = new();
		public List<string> FixedHeaders { get; init; } = new();
		public Func<string, Task>? OnReportGenerated { get; set; }

		public ChangeReportDbEventHandler<T> Build() {
			var options = TableOptions;
			if (options == null && !TableOptionFactory.Instance.TryGet<ChangeReport<T>>(out options)) {
				options = BuildDefaultTableOptions();
			}
			return new ChangeReportDbEventHandler<T> {
				ChangeType = Type,
				Prefix = Prefix,
				Postfix = Postfix,
				SkippedProperties = SkippedProperties.ToArray(),
				TableOptions = options,
				OnReportGenerated = OnReportGenerated,
			};
		}

		private TableOptions<ChangeReport<T>> BuildDefaultTableOptions() {
			var options = new TableOptions<ChangeReport<T>>();
			foreach (var header in FixedHeaders) {
				options.SetColumn(header, obj => typeof(ChangeReport<T>).GetPropertyValue(obj, "Entity." + header, false));
			}
			options.SetColumn(x => x.Property, x => x.Property);
			options.SetColumn(x => x.OldValue, x => x.OldValue);
			options.SetColumn(x => x.NewValue, x => x.NewValue);
			return options;
		}
	}
}