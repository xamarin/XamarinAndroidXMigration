using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xamarin.Android.Tools.Bytecode;

namespace grepinator
{
	public class SearchCommand : Command
	{
		public SearchCommand()
			: base("search", "Search the directory for a Java class.")
		{
			Options = new OptionSet
			{
				$"usage: {Program.Name} {Name} [OPTIONS]",
				"",
				Help,
				"",
				{ "d|directory=", "The directory to search for artifacts", v => AddDirectory(v) },
				{ "c|class=", "The Java class to find", v => JavaClasses.Add(v) },
				{ "?|h|help", "Show this message and exit", _ => ShowHelp = true },
			};
		}

		public bool ShowHelp { get; private set; }

		public List<string> Directories { get; } = new List<string>();

		public List<string> JavaClasses { get; } = new List<string>();

		public override int Invoke(IEnumerable<string> args)
		{
			try
			{
				var extra = Options.Parse(args);

				if (Directories.Count == 0)
					Directories.Add(Directory.GetCurrentDirectory());

				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}

				if (!ValidateArguments())
					return 1;

				DoSearch();

				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"{Program.Name}: An error occurred: `{ex.Message}`.");
				if (Program.Verbose)
					Console.Error.WriteLine(ex);
				return 1;
			}
		}

		private void AddDirectory(string directory)
		{
			if (string.IsNullOrWhiteSpace(directory))
				return;

			if (!Path.IsPathRooted(directory))
				directory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), directory));

			Directories.Add(directory);
		}

		private bool ValidateArguments()
		{
			var hasError = false;

			if (JavaClasses.Count == 0)
			{
				Console.Error.WriteLine($"{Program.Name}: At least one Java class is required `--class=PATH`.");
				hasError = true;
			}

			if (Directories.Count == 0)
			{
				Console.Error.WriteLine($"{Program.Name}: At least one directory is required `--directory=PATH`.");
				hasError = true;
			}

			var missing = Directories.Where(i => !Directory.Exists(i));

			if (missing.Any())
			{
				foreach (var file in missing)
					Console.Error.WriteLine($"{Program.Name}: Directory does not exist: `{file}`.");
				hasError = true;
			}

			if (hasError)
				Console.Error.WriteLine($"{Program.Name}: Use `{Program.Name} help {Name}` for details.");

			return !hasError;
		}

		private void DoSearch()
		{
			if (Program.Verbose)
			{
				Console.WriteLine($"Looking for the following classes:");
				foreach (var javaClass in JavaClasses)
					Console.WriteLine($" - {javaClass}");
				Console.WriteLine($"In the following directories:");
				foreach (var directory in Directories)
					Console.WriteLine($" - {directory}");
			}

			foreach (var directory in Directories)
			{
				var aarFiles = Directory.EnumerateFiles(directory, "*.aar", SearchOption.AllDirectories);
				var jarFiles = Directory.EnumerateFiles(directory, "*.jar", SearchOption.AllDirectories);
				var artifacts = jarFiles.Union(aarFiles);

				foreach (var artifact in artifacts)
				{
					if (Program.Verbose)
						Console.WriteLine($"Processing {artifact}...");

					foreach (var javaClass in JavaClasses)
					{
						if (string.IsNullOrWhiteSpace(javaClass))
							continue;

						Stream jarStream = null;
						try
						{
							if (Path.GetExtension(artifact).Equals(".aar", StringComparison.OrdinalIgnoreCase))
								jarStream = ReadZipEntry(artifact, "classes.jar");
							else if (Path.GetExtension(artifact).Equals(".jar", StringComparison.OrdinalIgnoreCase))
								jarStream = File.OpenRead(artifact);

							if (jarStream != null)
							{
								var classPath = new ClassPath();
								classPath.Load(jarStream);

								DoSearch(artifact, classPath, javaClass);
							}
						}
						finally
						{
							jarStream?.Dispose();
						}
					}
				}
			}
		}

		private void DoSearch(string artifact, ClassPath classPath, string javaClass)
		{
			foreach (var package in classPath.GetPackages())
			{
				var classFiles = package.Value;

				foreach (var classFile in classFiles)
				{
					var thisClass = classFile.ThisClass;
					if (thisClass.Name.Value.Contains(javaClass))
					{
						Console.WriteLine($"{thisClass.Name.Value}: {artifact}");
					}
				}
			}
		}

		public static Stream ReadZipEntry(string archivePath, string entryPath)
		{
			if (string.IsNullOrWhiteSpace(entryPath))
				return null;

			entryPath = entryPath.Replace("\\", "/");

			using (var archive = new ZipArchive(File.OpenRead(archivePath), ZipArchiveMode.Read, false))
			{
				var entry = archive.Entries.FirstOrDefault(e => e.FullName.Equals(entryPath, StringComparison.OrdinalIgnoreCase));
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
