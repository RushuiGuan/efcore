using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Albatross.EFCore {
	public class EFCoreSemanticExceptionConverter : ISemanticExceptionConverter{
		public virtual bool TryConvert(Exception source, [NotNullWhen(true)] out Exception? result) {
			if (source is DbUpdateConcurrencyException concurrencyEx) {
				result = new PreconditionFailedException(concurrencyEx.Message);
				return true;
			}
			result = null;
			return false;
		}
	}
}