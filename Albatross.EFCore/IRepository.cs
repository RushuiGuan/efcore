using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	public record SaveResults {
		public bool Success { get; init; }
		public bool NameConflict { get; init; }
		public bool ForeignKeyConflict { get; init; }
		public Exception? Error { get; init; }
	}

	public interface IRepository {
		Task<SaveResults> SaveChangesAsync(CancellationToken cancellationToken);
	}

	public abstract class Repository<T> : IRepository where T : IDbSession {
		protected readonly T session;
		protected Repository(T session) {
			this.session = session;
		}
		public abstract bool IsUniqueConstraintViolation(Exception err);
		public abstract bool IsForeignKeyConstraintViolation(Exception err);
		public async Task<SaveResults> SaveChangesAsync(CancellationToken cancellationToken) {
			try {
				await session.SaveChangesAsync(cancellationToken);
				return new SaveResults() { Success = true };
			} catch (Exception err) {
				bool hasNameConflict = IsUniqueConstraintViolation(err);
				bool hasForeignKeyConflict = false;
				if (!hasNameConflict) {
					hasForeignKeyConflict = IsForeignKeyConstraintViolation(err);
				}
				return new SaveResults {
					Success = false,
					Error = err,
					NameConflict = hasNameConflict,
					ForeignKeyConflict = hasForeignKeyConflict,
				};
			}
		}
	}
}