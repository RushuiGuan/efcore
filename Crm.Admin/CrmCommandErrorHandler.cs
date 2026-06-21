using Albatross.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;

namespace Crm.Admin {
	public class CrmCommandErrorHandler : ICommandErrorHandler {
		private readonly ICommandContext context;
		private readonly ILogger<CrmCommandErrorHandler> logger;

		public CrmCommandErrorHandler(ICommandContext context, ILogger<CrmCommandErrorHandler> logger) {
			this.context = context;
			this.logger = logger;
		}
		public int? Handle(Exception exception) {
			logger.LogError(exception, $"error executing command {context.Key}");
			while (exception.InnerException != null) {
				exception = exception.InnerException;
			}
			AnsiConsole.MarkupLineInterpolated($"[bold red]{exception.GetType().Name} :[/] {exception.Message}");
			return 1;
		}
	}
}
