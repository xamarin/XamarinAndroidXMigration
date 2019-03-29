using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Migration
{
	public class Jetifier
	{
		#region Constants

		private const string JetifierWrapperJarName = "JetifierWrapper.jar";
		private const string JetifierWrapperMain = "com.xamarin.androidx.jetifierWrapper.Main";

		#endregion

		#region Properties

		public string ConfigurationPath { get; set; }
		public bool Verbose { get; set; }
		public bool Dejetify { get; set; }
		public bool IsStrict { get; set; }
		public bool ShouldRebuildTopOfTree { get; set; }
		public bool ShouldStripSignatures { get; set; }
		public bool NoParallel { get; set; }
		public bool PrintHelp { get; set; }

		public string JavaPath { get; set; } = "java";

		public string JetifierWrapperPath { get; set; } = "Tools/JetifierWrapper/";

		#endregion

		#region Public Functionality

		public bool Jetify (MigrationPair archives) =>
			Jetify (new [] { archives });

		public bool Jetify (string source, string destination) =>
			Jetify (new [] { new MigrationPair (source, destination) });

		public bool Jetify (IEnumerable<MigrationPair> archives)
		{
			var assembly = typeof (Jetifier).Assembly;
			var jetifierWrapperRoot = Path.Combine (Path.GetDirectoryName (assembly.Location), JetifierWrapperPath);
			var jetifierWrapperJar = Path.Combine (jetifierWrapperRoot, JetifierWrapperJarName);
			var jars = Directory.GetFiles (jetifierWrapperRoot, "*.jar");

			var classPath = string.Join (Path.PathSeparator.ToString (), jars);
			var archiveArgs = archives.Select (pair => $"-i \"{pair.Source}\" -o \"{pair.Destination}\"");
			var c = !string.IsNullOrWhiteSpace (ConfigurationPath) ? $" -c \"{ConfigurationPath}\"" : "";
			var l = Verbose ? $" -l verbose" : "";
			var r = Dejetify ? " -r" : "";
			var s = IsStrict ? " -s" : "";
			var rebuildTopOfTree = ShouldRebuildTopOfTree ? " -rebuildTopOfTree" : "";
			var stripSignatures = ShouldStripSignatures ? " -stripSignatures" : "";
			var noParallel = NoParallel ? " -noParallel" : "";
			var h = PrintHelp ? " -h" : "";

			var proc = Process.Start (new ProcessStartInfo (JavaPath) {
				Arguments = $"-classpath \"{classPath}\" \"{JetifierWrapperMain}\" {string.Join (" ", archiveArgs)}{c}{l}{r}{s}{rebuildTopOfTree}{stripSignatures}{h}",
				RedirectStandardOutput = true
			});

			var output = proc.StandardOutput.ReadToEnd ();

			proc.WaitForExit ();

			var result = proc.ExitCode == 0;

			if (!result)
				throw new Exception ($"Java exited with error code: {proc.ExitCode}\n{output}");

			return result;
		}

		#endregion
	}
}
