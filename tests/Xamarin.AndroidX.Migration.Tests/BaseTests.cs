using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class BaseTests
	{
		public const string MergedSupportDll = "AndroidSupport.Merged.dll";
		public const string MergedAndroidXDll = "AndroidX.Merged.dll";

		public const string ManagedSupportDll = "Aarxercise.Managed.Support.dll";
		public const string ManagedAndroidXDll = "Aarxercise.Managed.AndroidX.dll";
		public const string BindingSupportDll = "Aarxercise.Binding.Support.dll";
		public const string BindingAndroidXDll = "Aarxercise.Binding.AndroidX.dll";

		public const string SupportAar = "aarxersise.java.support.aar";
		public const string AndroidXAar = "aarxersise.java.androidx.aar";

		public static Stream ReadAarEntry(string aarFilename, string entryFilename)
		{
			// convert to aar slashes
			entryFilename = entryFilename.Replace("\\", "/");

			using (var archive = new ZipArchive(File.OpenRead(aarFilename), ZipArchiveMode.Read, false))
			{
				var entry = archive.Entries.FirstOrDefault(e => e.FullName == entryFilename);

				if (entry != null)
				{
					using (var stream = entry.Open())
					{
						var output = new MemoryStream();
						stream.CopyTo(output);
						output.Position = 0;

						return output;
					}
				}
			}

			return null;
		}
	}
}
