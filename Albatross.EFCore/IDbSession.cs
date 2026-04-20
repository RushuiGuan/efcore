using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.EFCore {
	/// <summary>
	/// Represents an open database session — a unit of work over a live <see cref="IDbConnection"/>.
	/// Dispose when the request or operation scope is complete.
	/// </summary>
	/// <remarks>
	/// In the Albatross layered architecture, only repositories depend on <see cref="IDbSession"/>.
	/// Services depend on repository interfaces and must never reference <see cref="IDbSession"/> directly.
	/// </remarks>
	public interface IDbSession : IDisposable {
		DbContext DbContext { get; }
		IDbConnection DbConnection { get; }
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}