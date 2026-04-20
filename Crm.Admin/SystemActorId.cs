using Albatross.EFCore;
using System;

namespace Crm.Admin {
	public class SystemActorId : IGetCurrentActorId<Guid> {
		public static readonly Guid Value = new Guid("00000000-0000-0000-0000-000000000001");
		public Guid Get() => Value;
	}
}
