using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// Persistence contract shared by all aggregate-root repositories: save pending changes
	/// and manage entity lifetime within the current <see cref="IDbSession"/>.
	/// </summary>
	public interface IRepository {
		/// <summary>
		/// Persists all pending changes and translates known constraint violations into
		/// <see cref="SaveResults"/> flags.
		/// </summary>
		/// <param name="throwException">
		/// When false (typical for web requests), constraint violations are captured as
		/// <see cref="SaveResults.NameConflict"/> or <see cref="SaveResults.ForeignKeyConflict"/> flags.
		/// When true (typical for background jobs), any exception propagates directly.
		/// </param>
		Task<SaveResults> SaveChangesAsync(bool throwException, CancellationToken cancellationToken);

		void Add<T>(params IEnumerable<T> entity) where T : class;
		void Delete<T>(params IEnumerable<T> entity) where T : class;
	}

	/// <summary>
	/// Abstract base repository that handles <see cref="SaveResults"/> translation for
	/// database-specific constraint violations. Create one repository per aggregate root —
	/// not for every entity.
	/// </summary>
	/// <remarks>
	/// Derive per provider (SQL Server, PostgreSQL) and implement
	/// <see cref="IsUniqueConstraintViolation"/> and <see cref="IsForeignKeyConstraintViolation"/>
	/// using the extension methods from <c>Albatross.EFCore.SqlServer</c> or <c>Albatross.EFCore.PostgreSQL</c>:
	/// <code>
	/// public override bool IsUniqueConstraintViolation(Exception err) =>
	///     SqlServerExt.IsUniqueConstraintViolation(err) || PostgresExt.IsUniqueConstraintViolation(err);
	/// </code>
	/// Repositories hold <see cref="IDbSession"/> but never call <c>SaveChangesAsync</c> themselves —
	/// that is always the caller's responsibility. Always use <c>.Include()</c> for related data;
	/// lazy loading is disabled by convention.
	/// </remarks>
	public abstract class Repository<T> : IRepository where T : IDbSession {
		protected readonly T session;

		protected Repository(T session) {
			this.session = session;
		}

		/// <summary>
		/// Returns true if <paramref name="err"/> represents a unique-key constraint violation
		/// for the current database provider. Use the static helpers from
		/// <c>Albatross.EFCore.SqlServer</c> or <c>Albatross.EFCore.PostgreSQL</c>.
		/// </summary>
		public abstract bool IsUniqueConstraintViolation(Exception err);

		/// <summary>
		/// Returns true if <paramref name="err"/> represents a foreign-key constraint violation
		/// for the current database provider. Use the static helpers from
		/// <c>Albatross.EFCore.SqlServer</c> or <c>Albatross.EFCore.PostgreSQL</c>.
		/// </summary>
		public abstract bool IsForeignKeyConstraintViolation(Exception err);

		public async Task<SaveResults> SaveChangesAsync(bool throwException, CancellationToken cancellationToken) {
			try {
				await session.SaveChangesAsync(cancellationToken);
				return new SaveResults();
			} catch (Exception err) {
				if (throwException) {
					throw;
				}
				return HandleException(err);
			}
		}

		public SaveResults HandleException(Exception err) {
			bool hasForeignKeyConflict = false, hasNameConflict = false;
			var hasConcurrencyConflict = err is DbUpdateConcurrencyException || err.InnerException is DbUpdateConcurrencyException;
			if (!hasConcurrencyConflict) {
				hasNameConflict = IsUniqueConstraintViolation(err);
				if (!hasNameConflict) {
					hasForeignKeyConflict = IsForeignKeyConstraintViolation(err);
				}
			}
			return new SaveResults {
				Error = err.InnerException ?? err,
				NameConflict = hasNameConflict,
				ForeignKeyConflict = hasForeignKeyConflict,
				ConcurrencyConflict = hasConcurrencyConflict,
			};
		}

		public void Add<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().AddRange(entity);
		}

		public void Delete<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().RemoveRange(entity);
		}
	}
}