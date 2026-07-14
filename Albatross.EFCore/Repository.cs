using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	public interface IRepository {
		Task<int> SaveChangesAsync(CancellationToken cancellationToken);

		void Add<T>(params IEnumerable<T> entity) where T : class;
		void Delete<T>(params IEnumerable<T> entity) where T : class;
		ValueTask<T> GetRequired<T>(object[] keys, CancellationToken cancellationToken) where T : class;
		ValueTask<T?> Get<T>(object[] keys, CancellationToken cancellationToken) where T : class;
		Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
		/// <summary>
		/// Flushes pending changes and then commits the transaction, in that order. Commit only finalizes the
		/// database transaction — it does NOT flush the EF Core change tracker — so any change-tracker
		/// mutations made since the last save (added/modified/deleted entities) would be silently dropped if
		/// the transaction were committed without saving first. This method removes that footgun by pairing
		/// the two calls: after it returns, everything staged in the change tracker is guaranteed to be part
		/// of the committed transaction.
		/// </summary>
		/// <remarks>
		/// Prefer this over calling <see cref="SaveChangesAsync"/> and <see cref="IDbContextTransaction.CommitAsync"/>
		/// separately at the end of a unit of work. Note that operations executing their own SQL immediately
		/// (e.g. <c>ExecuteDeleteAsync</c>/<c>ExecuteUpdateAsync</c>) bypass the change tracker and are already
		/// applied within the transaction; the save here still runs so any tracked mutations alongside them are
		/// not lost.
		/// </remarks>
		/// <param name="transaction">The active transaction to commit, typically from <see cref="BeginTransactionAsync"/>.</param>
		/// <param name="cancellationToken">A token to cancel the save/commit.</param>
		Task SaveAndCommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken);
	}

	public abstract class Repository<T> : IRepository where T : IDbSession {
		protected readonly T session;
		private readonly ISemanticExceptionConverter semanticExceptionConverter;

		protected Repository(T session, ISemanticExceptionConverter semanticExceptionConverter) {
			this.session = session;
			this.semanticExceptionConverter = semanticExceptionConverter;
		}

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) {
			try {
				return await session.SaveChangesAsync(cancellationToken);
			} catch (Exception err) {
				if (this.semanticExceptionConverter.TryConvert(err, out var exception)) {
					throw exception;
				} else {
					throw;
				}
			}
		}

		public void Add<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().AddRange(entity);
		}

		public void Delete<TEntity>(params IEnumerable<TEntity> entity) where TEntity : class {
			this.session.DbContext.Set<TEntity>().RemoveRange(entity);
		}

		public async ValueTask<TEntity> GetRequired<TEntity>(object[] keys, CancellationToken cancellationToken) where TEntity : class {
			return await this.Get<TEntity>(keys, cancellationToken) ?? throw new NotFoundException<TEntity>(keys.Select(x => Convert.ToString(x) ?? string.Empty));
		}
		public ValueTask<TEntity?> Get<TEntity>(object[] keys, CancellationToken cancellationToken) where TEntity : class
			=> this.session.DbContext.Set<TEntity>().FindAsync(keys, cancellationToken);

		public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
			session.DbContext.Database.BeginTransactionAsync(cancellationToken);

		/// <inheritdoc />
		public async Task SaveAndCommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken) {
			// Save before commit: CommitAsync finalizes the transaction but does not flush the change tracker,
			// so committing without saving first would silently drop any tracked (added/modified/deleted) entities.
			await this.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
		}
	}
}