using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace JetifierWrapper {
	public class JetifyFiles : Task {

		#region Properties

		[Required]
		public string JetifierWrapperFullPath { get; set; }
		[Required]
		public ITaskItem [] Files { get; set; }
		[Output]
		public ITaskItem [] JetifiedFiles { get; set; }
		public string ConfigurationFullPath { get; set; }
		public string Verbosity { get; set; }
		public bool IsDejetify { get; set; }
		public bool IsStrict { get; set; }
		public bool IsRebuildTopOfTree { get; set; }
		public bool NoParallel { get; set; }
		public bool PrintHelp { get; set; }
		public bool OverrideFiles { get; set; }
		[Required]
		public bool IsUnix { get; set; }

		#endregion

		public JetifyFiles ()
		{
		}

		public override bool Execute ()
		{
			var files = new List<string> ();
			var jetifiedFiles = new List<string> ();
			var filesLength = Files.Length;

			JetifiedFiles = new ITaskItem [filesLength];
			Array.Copy (Files, JetifiedFiles, filesLength);

			for (int i = 0; i < filesLength; i++) {
				var currentFile = Files [i].ItemSpec;
				files.Add (currentFile);
				jetifiedFiles.Add (currentFile);

				if (!OverrideFiles) {
					var jetifiedFile = Path.GetDirectoryName (currentFile);
					jetifiedFile += Path.DirectorySeparatorChar;
					jetifiedFile += $"{Path.GetFileNameWithoutExtension (currentFile)}-jetified";
					jetifiedFile += Path.GetExtension (currentFile);

					jetifiedFiles [i] = jetifiedFile;
					JetifiedFiles [i].ItemSpec = jetifiedFile;
				}
			}

			var libDirectory = $"{Path.GetDirectoryName (JetifierWrapperFullPath)}{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}";
			var jarFiles = Directory.GetFiles (libDirectory, "*.jar", SearchOption.TopDirectoryOnly);

			var classpathJoinSymbol = IsUnix ? ":" : ";";
			var classPath = $"\"{JetifierWrapperFullPath}{classpathJoinSymbol}{string.Join (classpathJoinSymbol, jarFiles)}\"";
			var javaMainClass = "com.xamarin.androidx.jetifierWrapper.Main";
			var inputFiles = $"-i \"{string.Join ("\" -i \"", files)}\"";
			var oututFiles = $"-o \"{string.Join ("\" -o \"", jetifiedFiles)}\"";
			var configurationOption = string.IsNullOrWhiteSpace (ConfigurationFullPath) ? "" : $"-c {ConfigurationFullPath}";
			var verbosityOption = string.IsNullOrWhiteSpace (Verbosity) ? "" : $"-l {Verbosity}";
			var reversedOption = IsDejetify ? "-r" : "";
			var strictOption = IsStrict ? "-s" : "";
			var rebuildTopOfTreeOption = IsRebuildTopOfTree ? "-rebuildTopOfTree" : "";
			var noParallelOption = NoParallel ? "-noParallel" : "";
			var helpOption = PrintHelp ? "-h" : "";


			try {
				var processStartInfo = new ProcessStartInfo {
					FileName = "java",
					Arguments = $"-cp {classPath} {javaMainClass} {inputFiles} " +
						$"{oututFiles} {configurationOption} {verbosityOption} " +
						$"{reversedOption} {strictOption} {rebuildTopOfTreeOption} " +
						$"{noParallelOption} {helpOption}",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = $"{Path.GetDirectoryName (JetifierWrapperFullPath)}{Path.DirectorySeparatorChar}"
				};

				var process = new Process { StartInfo = processStartInfo };
				process.Start ();
				string result = process.StandardOutput.ReadToEnd ();
				Log.LogMessage (MessageImportance.Normal, result);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}

			return true;
		}


	}
}
