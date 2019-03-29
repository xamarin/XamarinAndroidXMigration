using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.AndroidX.Migration.BuildTasks {
	public class JetifyFiles : Task {

		#region Properties

		[Required]
		public ITaskItem [] Files { get; set; }
		[Output]
		public ITaskItem [] JetifiedFiles { get; set; }
		public string JetifierWrapperPath { get; set; }
		public string ConfigurationPath { get; set; }
		public bool Verbosity { get; set; }
		public bool Dejetify { get; set; }
		public bool IsStrict { get; set; }
		public bool ShouldRebuildTopOfTree { get; set; }
		public bool ShouldStripSignatures { get; set; }
		public bool NoParallel { get; set; }
		public bool PrintHelp { get; set; }
		public bool NoOverrideFiles { get; set; }

		#endregion

		public override bool Execute ()
		{
			var archives = new List<MigrationPair> ();

			for (int i = 0; i < Files.Length; i++) {
				var inputFile = Files [i].ItemSpec;
				var outputFile = inputFile;

				if (NoOverrideFiles) {
					outputFile = Path.GetDirectoryName (inputFile);
					outputFile += Path.DirectorySeparatorChar;
					outputFile += $"{Path.GetFileNameWithoutExtension (inputFile)}-jetified";
					outputFile += Path.GetExtension (inputFile);
				}

				archives.Add ((inputFile, outputFile));
			}

			var jetifier = new Jetifier {
				ConfigurationPath = ConfigurationPath,
				Verbose = Verbosity,
				Dejetify = Dejetify,
				IsStrict = IsStrict,
				ShouldRebuildTopOfTree = ShouldRebuildTopOfTree,
				ShouldStripSignatures = ShouldStripSignatures,
				NoParallel = NoParallel,
				PrintHelp = PrintHelp
			};

			try {
				jetifier.Jetify (archives);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}

			return true;
		}
	}
}
