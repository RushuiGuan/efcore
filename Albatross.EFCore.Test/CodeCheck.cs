using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Albatross.EFCore.Test {
	public class CodeCheck {
		[Theory]
		[InlineData(@"c:\temp", "a", @"c:\temp\a")]
		[InlineData(@"c:\temp", @"c:\a", @"c:\a")]
		public void Test(string p1, string p2, string expected) {
			var result = Path.Combine(p1, p2);
			Assert.Equal(expected, result);
		}
	}
}
