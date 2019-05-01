using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Mono.Cecil;
using System.Linq;

namespace Xamarin.AndroidX.Migration.BuildTasks {
	public class JetifyFiles : Task {

		// file inputs

		public ITaskItem [] Files { get; set; }
		public ITaskItem [] JetifiedFiles { get; set; }
		public string JetifiedDirectory { get; set; }

		// configuration inputs

		public string ConfigurationPath { get; set; }
		public bool Verbose { get; set; }
		public bool Dejetify { get; set; }
		public bool IsStrict { get; set; }
		public bool ShouldRebuildTopOfTree { get; set; }
		public bool ShouldStripSignatures { get; set; }
		public bool IsProGuard { get; set; }
		public bool NoParallel { get; set; }

		public override bool Execute ()
		{
			// make sure there is input
			if (Files == null || Files.Length == 0) {
				Log.LogError ($"Nothing to jetify. No files were provided via the \"{nameof (Files)}\" attribute.");
				return false;
			}

			// make sure the output files are valid
			if (JetifiedFiles?.Length > 0) {
				if (Files.Length != JetifiedFiles?.Length){
					Log.LogError ($"The length of {nameof (Files)} and {nameof (JetifiedFiles)} must be the same.");
					return false;
				}
				if (!string.IsNullOrEmpty(JetifiedDirectory)) {
					Log.LogError ($"The {nameof (JetifiedDirectory)} and {nameof (JetifiedFiles)} cannot both be set.");
					return false;
				}
			}

			try {
				var filesToJetify = CreateMigrationPairs ().ToList ();

				foreach (var file in filesToJetify)
					Log.LogMessage ($"Queuing jetification for {file.Source} to {file.Destination}.");

				var jetifier = new Jetifier {
					ConfigurationPath = ConfigurationPath,
					Verbose = Verbose,
					Dejetify = Dejetify,
					IsStrict = IsStrict,
					ShouldRebuildTopOfTree = ShouldRebuildTopOfTree,
					ShouldStripSignatures = ShouldStripSignatures,
					IsProGuard = IsProGuard,
					NoParallel = NoParallel,
				};

				if (!string.IsNullOrEmpty(JetifiedDirectory) && !Directory.Exists (JetifiedDirectory))
					Directory.CreateDirectory (JetifiedDirectory);

				jetifier.Jetify (filesToJetify);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex, true);
				return false;
			}

			return true;
		}

		private IEnumerable<MigrationPair> CreateMigrationPairs ()
		{
			var filesLength = Files?.Length ?? 0;
			for (int i = 0; i < filesLength; i++) {
				var inputFile = Files [i].ItemSpec;
				var outputFile = GetOutputFile (i) ?? inputFile;

				yield return (inputFile, outputFile);
			}
		}

		private string GetOutputFile (int index)
		{
			if (JetifiedFiles?.Length > index)
				return JetifiedFiles [index].ItemSpec;

			if (!string.IsNullOrEmpty (JetifiedDirectory))
				return Path.Combine (JetifiedDirectory, Guid.NewGuid ().ToString ());

			return null;
		}
	}
}
