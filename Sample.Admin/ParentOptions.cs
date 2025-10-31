using Albatross.CommandLine;

namespace Sample.Admin {
	[Verb("sqlserver", Description = "Execute sql server related commands")]
	[Verb("postgres", Description = "Execute postgresql server related commands")]
	public class ParentOptions {
	}
}