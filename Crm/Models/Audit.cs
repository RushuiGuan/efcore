using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Models {
	public class Audit : Albatross.EFCore.Change<Guid, Guid>{
	}
}
