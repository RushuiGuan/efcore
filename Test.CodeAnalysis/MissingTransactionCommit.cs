using Albatross.EFCore;

namespace Test.CodeAnalysis {
	public class MissingTransactionCommit{
		private readonly IRepository repository;

		public MissingTransactionCommit(IRepository repository) {
			this.repository = repository;
		}

		public async Task Case1(CancellationToken cancellation) {
			using var trasaction = await repository.BeginTransactionAsync(cancellation);
			await this.repository.SaveChangesAsync(cancellation);
		}

		public async Task Case2(CancellationToken cancellation) {
			await using var trasaction = await repository.BeginTransactionAsync(cancellation);
			await this.repository.SaveChangesAsync(cancellation);
		}

		public async Task Case3_Ok(CancellationToken cancellation) {
			await using var trasaction = await repository.BeginTransactionAsync(cancellation);
			await this.repository.SaveAndCommitAsync(trasaction, cancellation);
		}

		public async Task Case4_Ok(CancellationToken cancellation) {
			await using var trasaction = await repository.BeginTransactionAsync(cancellation);
			trasaction.Commit();
		}

		public async Task Case5_Ok(CancellationToken cancellation) {
			await using var trasaction = await repository.BeginTransactionAsync(cancellation);
			await trasaction.CommitAsync(cancellation);
		}

		public async Task Case6_Ok(CancellationToken cancellation) {
			await using var trasaction = await repository.BeginTransactionAsync(cancellation);
			try {
				await this.repository.SaveChangesAsync(cancellation);
				await trasaction.CommitAsync(cancellation);
			} catch {
				// error handling
			} finally {
				// cleanup
			}
		}
	}
}
