using Albatross.Config;
using Albatross.Text;
using Microsoft.Extensions.Configuration;

namespace Crm.Models.SqlServer {

	public class CrmConfig : ConfigBase, ICrmConfig {
		public CrmConfig(IConfiguration configuration) : base(configuration, null) {
			this.ConnectionString = configuration.GetRequiredConnectionString("sqlserver")
				.Interpolate(v => Environment.GetEnvironmentVariable(v) ?? throw new InvalidOperationException($"Environment variable ${v} is not found"));
		}
		public string ConnectionString { get; }
	}
}