using System;
using System.IO;
using NetMQ;

namespace SiPMTesterInterface.Helpers
{
	public static class KeyReader
	{
		//If relativePath is true, it combines the location of the executable with the given path
		//(making an absolute path for a given relative path)
		public static byte[] ReadKeyFile(string path, bool relativePath = false)
		{
            string baseDir = FilePathHelper.GetCurrentDirectory();
			string absPath = (relativePath ? Path.Combine(baseDir, path) : path);
            if (!File.Exists(absPath))
			{
				throw new FileNotFoundException("Key file not found, you should generate one first.");
			}
			return File.ReadAllBytes(absPath);
		}

		//relativePath is used for ReadKeyFile function
		public static NetMQCertificate ReadKeyFiles(string privKeyPath, string pubKeyPath, bool relativePath = false)
		{
			return new NetMQCertificate(ReadKeyFile(privKeyPath, relativePath), ReadKeyFile(pubKeyPath, relativePath));
		}
    }
}

