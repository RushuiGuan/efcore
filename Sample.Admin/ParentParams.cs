using Albatross.CommandLine.Annotations;

namespace Sample.Admin {
	[Verb("sqlserver", Description = "Execute sql server related commands")]
	[Verb("postgres", Description = "Execute postgresql server related commands")]
	[Verb("postgres contact", Description = "Commands related to contact management")]
	[Verb("sqlserver contact", Description = "Commands related to contact management")]
	[Verb("postgres address", Description = "Commands related to adddress management")]
	[Verb("sqlserver address", Description = "Commands related to adddress management")]
	public class ParentParams {
	}
}