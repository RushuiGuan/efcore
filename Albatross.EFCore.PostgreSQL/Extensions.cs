using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace Albatross.EFCore.PostgreSQL {
	public static class Extensions {
		public static void BuildDefaultOption(DbContextOptionsBuilder builder, string connectionString, IServiceProvider provider) {
			builder.EnableDetailedErrors(true);
			builder.EnableSensitiveDataLogging(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseNpgsql(connectionString, x => x.CommandTimeout(100));
			builder.UseLowerCaseNamingConvention();
		}

		public static DbContextOptions<T> BuildMigrationOption<T>(string historyTableSchema, string connectionString = DbSession.Any) where T : DbContext {
			var builder = new DbContextOptionsBuilder<T>();
			builder.EnableDetailedErrors(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseNpgsql(connectionString,
				opt => {
					opt.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
					// without this, efcore will create the historyTable using the default schema public
					opt.MigrationsHistoryTable(DbSession.EFMigrationHistory, historyTableSchema);
				});
			builder.UseLowerCaseNamingConvention();
			return builder.Options;
		}

		public static IServiceCollection AddPostgres<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, Action<NpgsqlDbContextOptionsBuilder>? npgSqlDbcontextOptionBuilder = null)
			where T : DbContext {
			services.AddDbContext<T>((provider, builder) => BuildDefaultOption(builder, getConnectionString(provider), provider));
			return services;
		}

		public static IServiceCollection AddPostgresWithContextPool<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString) where T : DbContext {
			services.AddDbContextPool<T>((provider, builder) => BuildDefaultOption(builder, getConnectionString(provider), provider));
			return services;
		}
	}
}