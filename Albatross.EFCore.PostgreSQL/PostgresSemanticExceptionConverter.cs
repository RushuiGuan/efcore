using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Albatross.EFCore.PostgreSQL {
	public class PostgresSemanticExceptionConverter : EFCoreSemanticExceptionConverter {
		public override bool TryConvert(Exception source, [NotNullWhen(true)] out Exception? result) {
			if (base.TryConvert(source, out result)) {
				return true;
			}
			var pgEx = source as PostgresException ?? source.InnerException as PostgresException;
			if (pgEx != null) {
				if (pgEx.SqlState == "23505") {
					result = new ConflictException(pgEx.Message);
					return true;
				}
				if (pgEx.SqlState == "23503") {
					if (source is DbUpdateException dbEx
						&& dbEx.Entries.Any(e => e.State == EntityState.Deleted)) {
						result = new ConflictException(pgEx.Message);
					} else {
						result = new NotFoundException(pgEx.Message);
					}
					return true;
				}
				if (pgEx.SqlState == "23001") {
					result = new ConflictException(pgEx.Message);
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}