using Albatross.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Albatross.EFCore.SqlServer {
	public class SqlServerSemanticExceptionConverter : EFCoreSemanticExceptionConverter{
		public override bool TryConvert(Exception source, [NotNullWhen(true)] out Exception? result) {
			if (base.TryConvert(source, out result)) {
				return true;
			}
			var sqlEx = source as SqlException ?? source.InnerException as SqlException;
			if (sqlEx != null) {
				if (sqlEx.Number is 2601 or 2627) {
					result = new ConflictException(sqlEx.Message);
					return true;
				}
				if (sqlEx.Number is 547) {
					if (source is DbUpdateException dbEx
						&& dbEx.Entries.Any(e => e.State == EntityState.Deleted)) {
						result = new ConflictException(sqlEx.Message);
					} else {
						result = new NotFoundException(sqlEx.Message);
					}
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}