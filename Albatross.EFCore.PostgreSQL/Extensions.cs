using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.EFCore.PostgreSQL {
	public static class Extensions {
		public static void BuildPostgresDbContext(DbContextOptionsBuilder builder, string connectionString, IServiceProvider provider, bool showSensitiveData) {
			builder.EnableDetailedErrors(true);
			builder.EnableSensitiveDataLogging(showSensitiveData);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseNpgsql(connectionString, x => x.CommandTimeout(100));
			builder.UseLowerCaseNamingConvention();
		}

		public static DbContextOptions<T> BuildPostgresMigrationDbContext<T>(string historyTableSchema, string connectionString = DbSession.Any) where T : DbContext {
			var builder = new DbContextOptionsBuilder<T>();
			builder.EnableDetailedErrors(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseNpgsql(connectionString,
				opt => {
					opt.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
					// without this, efcore will create the historyTable using the default schema public
					opt.MigrationsHistoryTable(DbSession.EFMigrationHistory, historyTableSchema);
				});
			// this one is postgres specific, lower case naming convention is postgres default, without it, efcore will create tables with proper case, which will require quoting when query
			builder.UseLowerCaseNamingConvention();
			return builder.Options;
		}

		public static IServiceCollection AddPostgres<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, bool showSensitiveData,
			Action<DbContextOptionsBuilder, IServiceProvider>? additionalConfig = null) where T : DbContext {
			services.AddDbContext<T>((provider, builder) => {
				BuildPostgresDbContext(builder, getConnectionString(provider), provider, showSensitiveData);
				if (additionalConfig != null) {
					additionalConfig(builder, provider);
				}
			});
			return services;
		}

		public static IServiceCollection AddPostgresWithContextPool<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, bool showSensitiveData,
			Action<DbContextOptionsBuilder, IServiceProvider>? additionalConfig = null) where T : DbContext {
			services.AddDbContextPool<T>((provider, builder) => {
				BuildPostgresDbContext(builder, getConnectionString(provider), provider, showSensitiveData);
				if (additionalConfig != null) {
					additionalConfig(builder, provider);
				}
			});
			return services;
		}
	}
}