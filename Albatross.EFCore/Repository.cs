using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	public interface IRepository {
		Task<int> SaveChangesAsync(CancellationToken cancellationToken);

		void Add<T>(params IEnumerable<T> entity) where T : class;
		void Delete<T>(params IEnumerable<T> entity) where T : class;
		Task<T> GetRequired<T>(object[] keys, CancellationToken cancellationToken) where T : class;
		Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
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

		public async Task<TEntity> GetRequired<TEntity>(object[] keys, CancellationToken cancellationToken) where TEntity : class {
			return await this.session.DbContext.Set<TEntity>().FindAsync(keys, cancellationToken) ?? throw new NotFoundException<TEntity>($"{keys}");
		}

		public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
			session.DbContext.Database.BeginTransactionAsync(cancellationToken);
	}
}