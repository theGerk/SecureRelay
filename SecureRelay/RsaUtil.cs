using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecureRelay
{
	public static class RsaUtil
	{
		public static async Task<RSACryptoServiceProvider> GetOrGeneratePrivateKey(string path, CancellationToken cancellationToken = default)
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			if (File.Exists(path))
				rsa.ImportCspBlob(await File.ReadAllBytesAsync(path, cancellationToken));
			else
				await File.WriteAllBytesAsync(path, rsa.ExportCspBlob(true), cancellationToken);
			return rsa;
		}

		public static string GetShaString(RSACryptoServiceProvider rsa)
		{
			return Convert.ToBase64String(SHA256.HashData(rsa.ExportCspBlob(false)));
		}
	}
}
