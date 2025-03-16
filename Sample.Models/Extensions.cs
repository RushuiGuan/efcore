using Albatross.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Models {
	public static class Extensions {
		public static IServiceCollection AddSampleDbSession(this IServiceCollection services) {
			services.AddDbSessionEvents();
			services.AddScoped<ISampleDbSession>(provider => provider.GetRequiredService<SampleDbSession>());
			return services;
		}
	}
}