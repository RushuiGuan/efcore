namespace Crm.Requests {
	public record class UpdateCompanyRequest {
		public string? NewName { get; init; }
		public string? Description { get; init; }
	}
}
