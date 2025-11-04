using Albatross.EFCore.Audit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Albatross.EFCore.ChangeReporting {
	public static class Extensions {
		public static ChangeReportBuilder<T> ExcludeAuditProperties<T>(this ChangeReportBuilder<T> builder) where T : class {
			builder.SkippedProperties.Add(nameof(IModifiedBy.ModifiedBy));
			builder.SkippedProperties.Add(nameof(IModifiedUtc.ModifiedUtc));
			builder.SkippedProperties.Add(nameof(ICreatedBy.CreatedBy));
			builder.SkippedProperties.Add(nameof(ICreatedUtc.CreatedUtc));
			return builder;
		}

		public static ChangeReportBuilder<T> ExcludeTemporalProperties<T>(this ChangeReportBuilder<T> builder) where T : class {
			builder.SkippedProperties.Add("PeriodStart");
			builder.SkippedProperties.Add("PeriodEnd");
			return builder;
		}

		public static IServiceCollection AddChangeReporting<T>(this IServiceCollection services, ChangeReportBuilder<T> builder) where T : class {
			services.TryAddEnumerable(ServiceDescriptor.Scoped<IDbSessionEventHandler, ChangeReportDbEventHandler<T>>(provider => builder.Build()));
			return services;
		}
		public static IServiceCollection AddChangeReporting<T>(this IServiceCollection services, Func<IServiceProvider, ChangeReportDbEventHandler<T>> func) where T : class {
			services.TryAddEnumerable(ServiceDescriptor.Scoped<IDbSessionEventHandler, ChangeReportDbEventHandler<T>>(func));
			return services;
		}
	}
}