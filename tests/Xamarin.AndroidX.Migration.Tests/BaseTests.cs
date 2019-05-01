using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

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

		public const string FacebookSdkZip = "facebook-android-sdk.zip";

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

		public static string ExtractAarEntry(string aarFilename, string entryFilename)
		{
			var destFile = Utils.GetTempFilename(entryFilename);

			using (var file = File.OpenWrite(destFile))
			using (var stream = ReadAarEntry(aarFilename, entryFilename))
			{
				stream.CopyTo(file);
			}

			return destFile;
		}

		public static string CreateFile(string filename, string contents)
		{
			var destFile = Utils.GetTempFilename(filename);

			File.WriteAllText(destFile, contents);

			return destFile;
		}

		public static string RunMigration(string supportDll, CecilMigrationResult expectedResult)
		{
			return RunCecilMigratorMigration(supportDll, expectedResult);
		}

		public static string RunCecilMigratorMigration(string supportDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = Utils.GetTempFilename();

			var migrator = new CecilMigrator();
			var result = migrator.Migrate(supportDll, migratedDll);

			Assert.Equal(expectedResult, result);

			return migratedDll;
		}
	}
}
