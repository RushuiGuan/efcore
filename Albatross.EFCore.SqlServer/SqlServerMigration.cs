using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Albatross.EFCore.SqlServer {
	public class SqlServerMigration<T> : Migration<T> where T : DbSession {
		private readonly ISqlBatchExecution batchExecution;

		public SqlServerMigration(ISqlBatchExecution batchExecution, T session, ILogger<SqlServerMigration<T>> logger) : base(session, logger){
			this.batchExecution = batchExecution;
		}
		protected override async Task Execute(string sql) {
			await batchExecution.Execute(session.DbConnection, new StringReader(sql));
		}
	}
}