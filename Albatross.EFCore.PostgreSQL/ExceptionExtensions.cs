using Npgsql;
using System;

namespace Albatross.EFCore.PostgreSQL {
	public static class ExceptionExtensions {
		// 23505: unique_violation
		public static bool IsUniqueConstraintViolation(this Exception err) {
			var pgEx = err as PostgresException ?? err.InnerException as PostgresException;
			return pgEx?.SqlState == "23505";
		}

		// 23503: foreign_key_violation
		public static bool IsForeignKeyConstraintViolation(this Exception err) {
			var pgEx = err as PostgresException ?? err.InnerException as PostgresException;
			return pgEx?.SqlState == "23503";
		}
	}
}