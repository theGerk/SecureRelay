using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Gerk.Crypto.EncryptedTransfer;

namespace SecureRelay
{
	public struct RsaKey : IHasPublicKeySha
	{
		public RsaKey(string base64)
		{
			key = new();
			key.ImportCspBlob(Convert.FromBase64String(base64));
		}

		public RsaKey(RSACryptoServiceProvider rsaKey = null)
		{
			key = rsaKey ?? new RSACryptoServiceProvider();
		}

		public readonly RSACryptoServiceProvider key;

		public byte[] GetPublicKeySha() => SHA256.HashData(key.ExportCspBlob(false));
	}
}
