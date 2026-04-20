using Microsoft.Data.SqlClient;
using System;

namespace Albatross.EFCore.SqlServer {
	public static class SqlServerExceptionExtensions {
		// 2601: Cannot insert duplicate key row (unique index)
		// 2627: Violation of PRIMARY KEY / UNIQUE KEY constraint
		public static bool IsUniqueConstraintViolation(this Exception err) {
			var sqlEx = err as SqlException ?? err.InnerException as SqlException;
			return sqlEx?.Number is 2601 or 2627;
		}

		// 547: INSERT/UPDATE/DELETE conflicted with FOREIGN KEY constraint
		public static bool IsForeignKeyConstraintViolation(this Exception err) {
			var sqlEx = err as SqlException ?? err.InnerException as SqlException;
			return sqlEx?.Number is 547;
		}

	}
}