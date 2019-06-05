using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Xamarin.AndroidX.Migration
{
	public class Jetifier
	{
		private const string JetifierWrapperJarName = "JetifierWrapper.jar";
		private const string JetifierWrapperMain = "com.xamarin.androidx.jetifierWrapper.Main";

		public string ConfigurationPath { get; set; }
		public bool Verbose { get; set; }
		public bool Dejetify { get; set; }
		public bool IsStrict { get; set; }
		public bool ShouldRebuildTopOfTree { get; set; }
		public bool ShouldStripSignatures { get; set; }
		public bool IsProGuard { get; set; }
		public bool Parallel { get; set; }
		public bool UseIntermediateFile { get; set; }
		public string IntermediateFilePath { get; set; }

		public string JavaPath { get; set; } = "java";

		public string JetifierWrapperPath { get; set; } = "Tools/JetifierWrapper/";

		public string LastOutput { get; private set; }

		public string LastError { get; private set; }

		public bool Jetify(MigrationPair archives) =>
			Jetify(new[] { archives });

		public bool Jetify(string source, string destination) =>
			Jetify(new[] { new MigrationPair(source, destination) });

		public bool Jetify(IEnumerable<MigrationPair> archives)
		{
			if (archives == null)
				throw new ArgumentNullException(nameof(archives), "There's nothing to jetify.");

			var assembly = typeof(Jetifier).Assembly;
			var jetifierWrapperRoot = Path.Combine(Path.GetDirectoryName(assembly.Location), JetifierWrapperPath);
			var jetifierWrapperJar = Path.Combine(jetifierWrapperRoot, JetifierWrapperJarName);
			var jars = Directory.GetFiles(jetifierWrapperRoot, "*.jar");

			var inputs = string.Empty;
			if (UseIntermediateFile)
			{
				if (string.IsNullOrWhiteSpace(IntermediateFilePath))
				{
					var tempRoot = Path.Combine(Path.GetTempPath(), "Xamarin.AndroidX.Migration", "Jetifier", Guid.NewGuid().ToString());
					IntermediateFilePath = tempRoot;
				}

				var dir = Path.GetDirectoryName(IntermediateFilePath);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				using (var intermediate = File.CreateText(IntermediateFilePath))
				{
					foreach (var file in archives)
					{
						intermediate.WriteLine($"{file.Source};{file.Destination}");
					}
				}

				inputs = $" -intermediate \"{IntermediateFilePath}\"";
			}
			else
			{
				foreach (var file in archives)
				{
					inputs += $" -i \"{file.Source}\" -o \"{file.Destination}\"";
				}
			}

			var classPath = string.Join(Path.PathSeparator.ToString(), jars);
			var c = !string.IsNullOrWhiteSpace(ConfigurationPath) ? $" -c \"{ConfigurationPath}\"" : string.Empty;
			var l = Verbose ? $" -l verbose" : string.Empty;
			var r = Dejetify ? " -r" : string.Empty;
			var s = IsStrict ? " -s" : string.Empty;
			var rebuildTopOfTree = ShouldRebuildTopOfTree ? " -rebuildTopOfTree" : string.Empty;
			var stripSignatures = ShouldStripSignatures ? " -stripSignatures" : string.Empty;
			var isProGuard = IsProGuard ? " -isProGuard" : string.Empty;
			var parallel = Parallel ? " -parallel" : string.Empty;
			var options = $"{c}{l}{r}{s}{rebuildTopOfTree}{stripSignatures}{isProGuard}{parallel}";
			var arguments = $"-classpath \"{classPath}\" \"{JetifierWrapperMain}\" {inputs} {options}";

			if (Verbose)
			{
				Console.WriteLine($"Running jetifier:");
				Console.WriteLine($"  JavaPath: {JavaPath}");
				Console.WriteLine($"  Arguments {arguments.Length}: {arguments}");
			}

			var proc = Process.Start(new ProcessStartInfo(JavaPath)
			{
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			});

			LastOutput = proc.StandardOutput.ReadToEnd();
			LastError = proc.StandardError.ReadToEnd();

			proc.WaitForExit();

			var result = proc.ExitCode == 0;

			if (!result)
				throw new Exception($"Java exited with error code: {proc.ExitCode}\nError: {LastError}\nOutput: {LastOutput}");

			return result;
		}
	}
}
