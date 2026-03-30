using Albatross.CommandLine.Annotations;

namespace Crm.Admin {
	[Verb("sqlserver", Alias =["sql"], Description = "Execute sql server related commands")]
	[Verb("postgres", Alias =["pg"], Description = "Execute postgresql server related commands")]
	public class ParentParams {
	}
}