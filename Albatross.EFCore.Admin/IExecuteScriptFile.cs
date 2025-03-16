using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Albatross.EFCore.Admin {
	public interface IExecuteScriptFile {
		Task ExecuteAsync(DbContext context, string filename);
	}
}