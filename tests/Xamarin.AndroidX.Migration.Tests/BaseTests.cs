using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class BaseTests
	{
		public const string ManagedSupportDll = "Aarxercise.Managed.Support.dll";
		public const string ManagedAndroidXDll = "Aarxercise.Managed.AndroidX.dll";
		public const string BindingSupportDll = "Aarxercise.Binding.Support.dll";
		public const string BindingAndroidXDll = "Aarxercise.Binding.AndroidX.dll";

		public const string SupportAar = "aarxersise.java.support.aar";
		public const string AndroidXAar = "aarxersise.java.androidx.aar";

		public const string JetifierWrapperZip = "JetifierWrapper.zip";
		public const string JetifierWrapperMain = "com.xamarin.androidx.jetifierWrapper.Main";

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

		public static (string Input, string Output)[] RunJetifierWrapper(params string[] inputs)
		{
			var dest = Path.Combine("JetifierWrapper", "JetifierWrapper.jar");

			if (!File.Exists(dest))
				ZipFile.ExtractToDirectory(JetifierWrapperZip, "JetifierWrapper");

			var files = inputs
				.Select(aar => (Input: aar, Output: Path.ChangeExtension(aar, "jetified.aar")))
				.ToArray();

			var args = files.Select(pair => $"-i \"{pair.Input}\" -o \"{pair.Output}\"");

			var jars = Directory.GetFiles("JetifierWrapper", "*.jar");
			var classPath = string.Join(Path.PathSeparator, jars);

			var proc = Process.Start(new ProcessStartInfo("java")
			{
				Arguments = $"-classpath \"{classPath}\" {JetifierWrapperMain} {string.Join(" ", args)}",
				RedirectStandardOutput = true
			});

			var output = proc.StandardOutput.ReadToEnd();

			proc.WaitForExit();

			if (proc.ExitCode != 0)
				throw new Exception($"Java exited with error code: {proc.ExitCode}\n{output}");

			return files;
		}
	}
}
