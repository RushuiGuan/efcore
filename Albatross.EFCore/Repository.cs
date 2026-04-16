using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	public record SaveResults {
		[MemberNotNullWhen(false, nameof(Error))]
		public bool Success => Error == null;

		public bool NameConflict { get; init; }
		public bool ForeignKeyConflict { get; init; }
		public Exception? Error { get; init; }
	}

	public interface IRepository {
		Task<SaveResults> SaveChangesAsync(bool throwException, CancellationToken cancellationToken);
		void Add<T>(params IEnumerable<T> entity) where T : class;
		void Delete<T>(params IEnumerable<T> entity) where T : class;
	}

	public abstract class Repository<T> : IRepository where T : IDbSession {
		protected readonly T session;
		protected Repository(T session) {
			this.session = session;
		}
		public abstract bool IsUniqueConstraintViolation(Exception err);
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