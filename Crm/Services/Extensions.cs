using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Services {
	public static class Extensions {
		public static IServiceCollection AddCrm(this IServiceCollection services) {
			services.AddScoped<ICompanyService, CompanyService>();
			return services;
		}
	}
}