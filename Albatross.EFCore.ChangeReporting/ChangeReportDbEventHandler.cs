using Albatross.Text;
using Albatross.Text.Table;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Albatross.EFCore.ChangeReporting {
	public class ChangeReportDbEventHandler<T> : IDbSessionEventHandler where T : class {
		public required ChangeType ChangeType { get; init; }
		public required string? Prefix { get; init; }
		public required string? Postfix { get; init; }
		public required TableOptions<ChangeReport<T>> TableOptions { get; init; }
		public required string[] SkippedProperties { get; init; }
		public required Func<string, Task>? OnReportGenerated { get; init; }

		private readonly List<ChangeReport<T>> changes = new();
		public string Text { get; private set; } = string.Empty;
		public override string ToString() => $"{typeof(T).Name} {nameof(ChangeReportDbEventHandler<T>)}";

		public Task PreSave(IDbSession session) => Task.CompletedTask;

		public async Task PostSave() {
			if (this.changes.Any()) {
				this.Text = BuildText();
				if (!string.IsNullOrEmpty(Text) && OnReportGenerated != null) {
					await OnReportGenerated(this.Text);
				}
			}
		}

		protected virtual string BuildText() {
			var writer = new StringWriter();
			var selected = this.changes.Where(x => !this.SkippedProperties.Contains(x.Property)).ToArray();
			if (this.Prefix != null) { writer.Append(Prefix); }
			var stringTable = new StringTable(selected, this.TableOptions);
			stringTable.Print(writer, true, true);
			if (!string.IsNullOrEmpty(this.Postfix)) {
				writer.Append(Postfix);
			}
			return writer.ToString();
		}

		public void OnAddedEntry(EntityEntry entry) {
			if (entry.Entity is T entity && (ChangeType & ChangeType.Added) > 0) {
				changes.AddRange(entry.Properties
					.Where(args => !SkippedProperties.Contains(args.Metadata.Name))
					.Select(args => new ChangeReport<T>(entity, args.Metadata.Name) {
						OldValue = null,
						NewValue = args.CurrentValue,
					}));
			}
		}

		public void OnModifiedEntry(EntityEntry entry) {
			if (entry.Entity is T entity && (ChangeType & ChangeType.Modified) > 0) {
				changes.AddRange(entry.Properties
					.Where(args => args.IsModified && !SkippedProperties.Contains(args.Metadata.Name))
					.Select(args => new ChangeReport<T>(entity, args.Metadata.Name) {
						OldValue = args.OriginalValue,
						NewValue = args.CurrentValue,
					}));
			}
		}

		public void OnDeletedEntry(EntityEntry entry) {
			if (entry.Entity is T entity && (ChangeType & ChangeType.Deleted) > 0) {
				changes.AddRange(entry.Properties
					.Where(args => !SkippedProperties.Contains(args.Metadata.Name))
					.Select(args => new ChangeReport<T>(entity, args.Metadata.Name) {
						OldValue = args.CurrentValue,
						NewValue = null,
					}));
			}
		}
	}
}