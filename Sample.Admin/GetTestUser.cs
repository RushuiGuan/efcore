using Albatross.Authentication;

namespace Sample.Admin {
	public class GetTestUser : IGetCurrentUser {
		public string Get() => "TestUser";
		public string Provider => "Test";
	}
}