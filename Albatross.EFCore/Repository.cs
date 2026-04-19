using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// The outcome of a <see cref="IRepository.SaveChangesAsync"/> call, including structured
	/// constraint-violation flags so callers can respond without parsing exception messages.
	/// </summary>
	/// <remarks>
	/// Callers (controllers or command handlers) handle <see cref="SaveResults"/> at the application
	/// boundary — after all service calls in the unit of work are complete:
	/// <code>
	/// var result = await repository.SaveChangesAsync(throwException: false, cancellationToken);
	/// if (!result.Success) {
	///     if (result.NameConflict) return Conflict("Name already exists");
	///     if (result.ForeignKeyConflict) return UnprocessableEntity("Referenced entity not found");
	///     throw result.Error!;
	/// }
	/// </code>
	/// </remarks>
	public record SaveResults {
		[MemberNotNullWhen(false, nameof(Error))]
		public bool Success => Error == null;

		/// <summary>True when the save failed due to a unique-key constraint violation.</summary>
		public bool NameConflict { get; init; }

		/// <summary>True when the save failed due to a foreign-key constraint violation.</summary>
		public bool ForeignKeyConflict { get; init; }

		/// <summary>
		/// The exception that caused the save to fail — the inner exception if one exists, otherwise the
		/// original. Null when <see cref="Success"/> is true.
		/// </summary>
		public Exception? Error { get; init; }
	}

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
				bool hasNameConflict = IsUniqueConstraintViolation(err);
				bool hasForeignKeyConflict = false;
				if (!hasNameConflict) {
					hasForeignKeyConflict = IsForeignKeyConstraintViolation(err);
				}
				return new SaveResults {
					Error = err.InnerException ?? err,
					NameConflict = hasNameConflict,
					ForeignKeyConflict = hasForeignKeyConflict,
				};
			}
		}
		public void Add<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().AddRange(entity);
		}
		public void Delete<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().RemoveRange(entity);
		}
	}
}
