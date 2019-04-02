using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Mono.Cecil;

namespace Xamarin.AndroidX.Migration.BuildTasks {
	public class JetifyFiles : Task {

		#region Class Variables

		readonly string jetifiedSuffix = "-jetified";
		readonly string [] resourcesName = { "__AndroidLibraryProjects__.zip" };
		readonly string cachePath = Path.GetTempPath ();

		#endregion

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
			var archivesToJetify = new List<MigrationPair> ();
			var archivesToEmbed = new List<ArchivesPair> ();

			for (int i = 0; i < Files.Length; i++) {
				var inputFile = Files [i].ItemSpec;
				var outputFile = inputFile;

				// If .dll, we need to extract the zip file and jetify it.
				//if (inputFile.EndsWith (".dll", true, CultureInfo.CurrentCulture)) {
				//	var dllFile = inputFile;
				//	var resources = new List<string> ();

				//	// Extract the zip file and add it to the list to jetify
				//	foreach (var resourceName in resourcesName) {
				//		inputFile = Path.Combine (cachePath, Path.GetFileNameWithoutExtension (dllFile), resourceName);
				//		outputFile = inputFile;

				//		if (!ExtractResourceToDiskIfExists (dllFile, resourceName, inputFile))
				//			continue;

				//		if (NoOverrideFiles)
				//			dllFile = AddJetifiedSufix (dllFile);

				//		archivesToJetify.Add ((inputFile, outputFile));
				//		resources.Add (outputFile);
				//	}

				//	if (resources.Count > 0)
				//		archivesToEmbed.Add ((dllFile, resources));
				//} else {
					if (NoOverrideFiles)
						outputFile = AddJetifiedSufix (inputFile);

					archivesToJetify.Add ((inputFile, outputFile));
				//}
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
				jetifier.Jetify (archivesToJetify);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}

			//foreach (var archives in archivesToEmbed) {
			//	foreach (var archive in archives.Archives)
			//		if (!ReplaceResource (archives.Source, archive))
			//			return false;
			//}

			return true;
		}

		#region Internal Functionality

		// Add the jetify suffix to the path
		public string AddJetifiedSufix (string path)
		{
			var jetifiedPath = Path.GetDirectoryName (path);
			jetifiedPath += Path.DirectorySeparatorChar;
			jetifiedPath += $"{Path.GetFileNameWithoutExtension (path)}{jetifiedSuffix}";
			jetifiedPath += Path.GetExtension (path);
			return jetifiedPath;
		}

		public string RemoveJetifiedSufix (string path)
		{
			var unjetifiedPath = Path.GetDirectoryName (path);
			unjetifiedPath += Path.DirectorySeparatorChar;
			unjetifiedPath += $"{Path.GetFileNameWithoutExtension (path)}".Replace (jetifiedSuffix, "");
			unjetifiedPath += Path.GetExtension (path);
			return unjetifiedPath;
		}

		public bool CreateJetifiedFile (string filePath)
		{
			var unjetifiedPath = RemoveJetifiedSufix (filePath);

			try {
				using (var unjetifiedStream = File.Open (unjetifiedPath, FileMode.Open))
				using (var fileStream = File.Open (filePath, FileMode.Create)) {
					unjetifiedStream.CopyTo (fileStream);
				}
				return true;
			} catch (Exception ex) {
				Log.LogError ($"Could not create the jetified {filePath} file.\n{ex.Message}");
				return false;
			}

		}

		// Extract the resource to the disk
		public bool ExtractResourceToDiskIfExists (string dllPath, string resourceName, string resourcePath)
		{
			var assembly = AssemblyDefinition.ReadAssembly (dllPath);

			foreach (var resource in assembly.MainModule.Resources) {
				if (resource.Name.ToLower () != resourceName.ToLower ())
					continue;

				try {
					using (var resourceStream = File.Open (resourcePath, FileMode.Create)) {
						var embeddedResource = (EmbeddedResource)resource;
						var stream = embeddedResource.GetResourceStream ();
						stream.CopyTo (resourceStream);
					}
					return true;
				} catch (Exception ex) {
					Log.LogWarning ($"Could not extract the {resourceName} resource file from {dllPath} and save it into {resourcePath}.\nException message: {ex.Message}");
					return false;
				}
			}

			Log.LogWarning ($"Could not find {resourceName} resource file inside of {dllPath}");

			return false;
		}

		// Deletes the resource from dll and embed the new one
		public bool ReplaceResource (string dllPath, string resourcePath)
		{
			if (!File.Exists (resourcePath)) {
				Log.LogError ($"The jetified {resourcePath} resource file to embed into {dllPath} does not exist.");
				return false;
			}

			var resourceName = Path.GetFileName (resourcePath);
			var definition = AssemblyDefinition.ReadAssembly (dllPath);

			for (var i = 0; i < definition.MainModule.Resources.Count; i++) {
				if (definition.MainModule.Resources [i].Name != resourceName)
					continue;

				definition.MainModule.Resources.RemoveAt (i);
				break;
			}

			try {
				using (var resourceStream = File.Open (resourcePath, FileMode.Open)) {
					var embeddedResource = new EmbeddedResource (resourceName, ManifestResourceAttributes.Public, resourceStream);
					definition.MainModule.Resources.Add (embeddedResource);
					definition.Write (dllPath);
				}
				return true;
			} catch (Exception ex) {
				Log.LogError ($"Could not embed the jetified {resourcePath} resource file into {dllPath}.\n{ex.Message}");
				return false;
			}
		}

		#endregion
	}
}
