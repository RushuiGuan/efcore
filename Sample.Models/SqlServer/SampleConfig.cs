using Albatross.Config;
using Albatross.Text;
using Microsoft.Extensions.Configuration;

namespace Sample.Models.SqlServer {
	public class SampleConfig : ConfigBase, ISampleConfig {
		public SampleConfig(IConfiguration configuration) : base(configuration, null) {
			this.ConnectionString = configuration.GetRequiredConnectionString("sqlserver")
				.Interpolate(v => Environment.GetEnvironmentVariable(v) ?? throw new InvalidOperationException($"Environment variable ${v} is not found"));
		}
		public string ConnectionString { get; }
	}
}