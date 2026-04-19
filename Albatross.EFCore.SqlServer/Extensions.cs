using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.EFCore.SqlServer {
	public static class Extensions {
		public static void BuildSqlServerDbContext(this DbContextOptionsBuilder builder,
			string connectionString,
			IServiceProvider serviceProvider, bool showSensitiveData) {
			builder.EnableDetailedErrors(true);
			builder.EnableSensitiveDataLogging(showSensitiveData);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseSqlServer(connectionString, x => x.CommandTimeout(100));
		}

		public static DbContextOptions<T> BuildSqlServerMigrationDbContext<T>(string historyTableSchema, string connectionString = DbSession.Any) where T : DbContext {
			var builder = new DbContextOptionsBuilder<T>();
			builder.EnableDetailedErrors(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseSqlServer(connectionString,
				opt => {
					opt.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
					opt.MigrationsHistoryTable(DbSession.EFMigrationHistory, historyTableSchema);
				});
			return builder.Options;
		}

		public static IServiceCollection AddSqlServer<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, bool showSensitiveData,
			Action<DbContextOptionsBuilder, IServiceProvider>? additionalConfig = null) where T : DbContext {
			services.AddDbContext<T>((provider, builder) => {
				BuildSqlServerDbContext(builder, getConnectionString(provider), provider, showSensitiveData);
				if (additionalConfig != null) {
					additionalConfig(builder, provider);
				}
			});
			return services;
		}

		/// <summary>
		/// when using context pool, the dbcontext class has to have a constructor with only dbcontextoption parameter 
		/// To create a readonly connection, use the readonly connection string if available (require sql server availability group).  
		/// As an alternative, access the database using an user with only readonly permission
		/// </summary>
		/// <returns></returns>
		public static IServiceCollection AddSqlServerWithContextPool<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, bool showSensitiveData,
			Action<DbContextOptionsBuilder, IServiceProvider>? additionalConfig = null) where T : DbContext {
			services.AddDbContextPool<T>((provider, builder) => {
				BuildSqlServerDbContext(builder, getConnectionString(provider), provider, showSensitiveData);
				if (additionalConfig != null) {
					additionalConfig(builder, provider);
				}
			});
			return services;
		}
	}
}