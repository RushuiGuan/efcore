using Albatross.Config;
using Albatross.Text;
using Microsoft.Extensions.Configuration;

namespace Sample.Models.Postgres {
	public class SampleConfig : ConfigBase, ISampleConfig {
		public SampleConfig(IConfiguration configuration) : base(configuration, null) {
			this.ConnectionString = configuration.GetRequiredConnectionString("postgres")
				.Interpolate(v => Environment.GetEnvironmentVariable(v) ?? throw new InvalidOperationException($"Environment variable ${v} is not found"));
		}
		public string ConnectionString { get;  }
	}
}