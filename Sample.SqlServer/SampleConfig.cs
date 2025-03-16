using Albatross.Config;
using Albatross.Text;
using Microsoft.Extensions.Configuration;

namespace Sample.SqlServer {
	public class SampleConfig : ConfigBase {
		public SampleConfig(IConfiguration configuration) : base(configuration) {
			this.ConnectionString = configuration.GetRequiredConnectionString("sqlserver")
				.Interpolate(v => Environment.GetEnvironmentVariable(v) ?? throw new InvalidOperationException($"Environment variable ${v} is not found"));
		}
		public string ConnectionString { get; }
	}
}