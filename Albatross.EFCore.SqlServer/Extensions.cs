using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.EFCore.SqlServer {
	public static class Extensions {
		public static void DefaultSqlServerDbContextOptionBuilder(SqlServerDbContextOptionsBuilder builder) {
			builder.CommandTimeout(100);
		}

		public static void BuildDefaultOption(this DbContextOptionsBuilder builder, string connectionString, Action<SqlServerDbContextOptionsBuilder>? sqlserverDbcontextOptionBuilder = null) {
			builder.EnableDetailedErrors(true);
			builder.EnableSensitiveDataLogging(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseSqlServer(connectionString, sqlserverDbcontextOptionBuilder ?? DefaultSqlServerDbContextOptionBuilder);
		}

		public static DbContextOptions<T> BuildMigrationOption<T>(string historyTableSchema, string connectionString = DbSession.Any) where T : DbContext {
			var builder = new DbContextOptionsBuilder<T>();
			builder.EnableDetailedErrors(true);
			builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			builder.UseSqlServer(connectionString, opt => {
				opt.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
				opt.MigrationsHistoryTable(DbSession.EFMigrationHistory, historyTableSchema);
			});
			return builder.Options;
		}

		public static IServiceCollection AddSqlServer<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, Action<IServiceProvider, DbContextOptionsBuilder>? dbContextOptionBuilder = null, Action<SqlServerDbContextOptionsBuilder>? sqlServerDbContextOptionBuilder = null) where T : DbContext {
			services.AddDbContext<T>((provider, builder) => BuildDefaultOption(builder, getConnectionString(provider), sqlServerDbContextOptionBuilder ?? DefaultSqlServerDbContextOptionBuilder));
			return services;
		}

		/// <summary>
		/// when using context pool, the dbcontext class has to have a constructor with only dbcontextoption parameter 
		/// To create a readonly connection, use the readonly connection string if available (require sql server availability group).  
		/// As an alternative, access the database using an user with only readonly permission
		/// </summary>
		/// <returns></returns>
		public static IServiceCollection AddSqlServerWithContextPool<T>(this IServiceCollection services, Func<IServiceProvider, string> getConnectionString, Action<IServiceProvider, DbContextOptionsBuilder>? dbContextOptionBuilder = null, Action<SqlServerDbContextOptionsBuilder>? sqlServerDbContextOptionBuilder = null) where T : DbContext {
			services.AddDbContextPool<T>((provider, builder) => BuildDefaultOption(builder, getConnectionString(provider), sqlServerDbContextOptionBuilder ?? DefaultSqlServerDbContextOptionBuilder));
			return services;
		}

		// 2601: Cannot insert duplicate key row (unique index)
		// 2627: Violation of PRIMARY KEY / UNIQUE KEY constraint
		public static bool IsUniqueConstraintViolation(this Exception err) {
			var sqlEx = err as SqlException ?? err.InnerException as SqlException;
			return sqlEx?.Number is 2601 or 2627;
		}

		// 547: INSERT/UPDATE/DELETE conflicted with FOREIGN KEY constraint
		public static bool IsForeignKeyConstraintViolation(this Exception err) {
			var sqlEx = err as SqlException ?? err.InnerException as SqlException;
			return sqlEx?.Number is 547;
		}
	}
}