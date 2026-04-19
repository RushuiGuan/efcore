using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Requests {
	public record class CreateCompanyRequest {
		public required string Name { get; init; }
		public string? Description { get; init; }
	}
}