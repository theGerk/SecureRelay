using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gerk.Crypto.EncryptedTransfer;

namespace SecureRelay
{
	internal static class Persmissions
	{
		public static bool HasPermission(IHasPublicKeySha id)
		{
			return true;
		}
	}
}
