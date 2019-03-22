using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Migration
{
	public class Jetifier
	{
		private const string JetifierWrapperJarName = "JetifierWrapper.jar";

		private const string JetifierWrapperMain = "com.xamarin.androidx.jetifierWrapper.Main";

		public Jetifier()
		{
		}

		public bool Verbose { get; set; }

		public string JavaPath { get; set; } = "java";

		public string JetifierWrapperPath { get; set; } = "Tools/JetifierWrapper/";

		public bool Jetify(MigrationPair archives) =>
			Jetify(new[] { archives });

		public bool Jetify(string source, string destination) =>
			Jetify(new[] { new MigrationPair(source, destination) });

		public bool Jetify(IEnumerable<MigrationPair> archives)
		{
			var assembly = typeof(Jetifier).Assembly;
			var jetifierWrapperRoot = Path.Combine(Path.GetDirectoryName(assembly.Location), JetifierWrapperPath);
			var jetifierWrapperJar = Path.Combine(jetifierWrapperRoot, JetifierWrapperJarName);

			var archiveArgs = archives.Select(pair => $"-i \"{pair.Source}\" -o \"{pair.Destination}\"");

			var jars = Directory.GetFiles(jetifierWrapperRoot, "*.jar");
			var classPath = string.Join(Path.PathSeparator.ToString(), jars);

			var proc = Process.Start(new ProcessStartInfo(JavaPath)
			{
				Arguments = $"-classpath \"{classPath}\" \"{JetifierWrapperMain}\" {string.Join(" ", archiveArgs)}",
				RedirectStandardOutput = true
			});

			var output = proc.StandardOutput.ReadToEnd();

			proc.WaitForExit();

			var result = proc.ExitCode == 0;

			if (!result)
				throw new Exception($"Java exited with error code: {proc.ExitCode}\n{output}");

			return result;
		}
	}
}
