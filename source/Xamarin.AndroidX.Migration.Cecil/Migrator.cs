using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.IO;

namespace Xamarin.AndroidX.Migration.Cecil
{
	public class Migrator
	{
		public Migrator()
			: this(CsvMapping.Instance)
		{
		}

		public Migrator(Stream stream)
			: this(new CsvMapping(stream))
		{
		}

		public Migrator(CsvMapping mapping)
		{
			Mapping = CsvMapping.Instance;
		}

		public CsvMapping Mapping { get; }

		public MigrationResult Migrate(AssemblyPair[] assemblies, bool verbose)
		{
			var result = MigrationResult.Skipped;

			foreach (var pair in assemblies)
			{
				result |= Migrate(pair.Source, pair.Destination, verbose);
			}

			return result;
		}

		public MigrationResult Migrate(AssemblyPair assemblies, bool verbose) =>
			Migrate(assemblies.Source, assemblies.Destination, verbose);

		public MigrationResult Migrate(string source, string destination, bool verbose)
		{
			if (string.IsNullOrWhiteSpace(source))
				throw new ArgumentException($"Invalid source assembly path specified: '{source}'.", nameof(source));
			if (string.IsNullOrWhiteSpace(destination))
				throw new ArgumentException($"Invalid destination assembly path specified: '{destination}'.", nameof(destination));
			if (!File.Exists(source))
				throw new FileNotFoundException($"Source assembly does not exist.", source);

			var pdbPath = Path.ChangeExtension(source, "pdb");
			var destPdbPath = Path.ChangeExtension(destination, "pdb");
			var tempDllPath = Path.ChangeExtension(destination, "temp.dll");
			var tempPdbPath = Path.ChangeExtension(destination, "temp.pdb");

			var hasPdb = File.Exists(pdbPath);
			var result = MigrationResult.Skipped;

			using (var resolver = new DefaultAssemblyResolver())
			{
				resolver.AddSearchDirectory(Path.GetDirectoryName(source));
				var readerParams = new ReaderParameters
				{
					ReadSymbols = hasPdb,
					AssemblyResolver = resolver,
				};

				var requiresSave = false;

				using (var assembly = AssemblyDefinition.ReadAssembly(source, readerParams))
				{
					if (verbose)
						Console.WriteLine($"Processing assembly '{source}'...");

					result = MigrateAssembly(assembly, verbose);

					requiresSave =
						result.HasFlag(MigrationResult.ContainedSupport) ||
						result.HasFlag(MigrationResult.ContainedJni);

					if (requiresSave)
					{
						Stream symbolStream = null;
						if (hasPdb)
							symbolStream = File.Create(tempPdbPath);

						try
						{
							var dir = Path.GetDirectoryName(destination);
							if (!Directory.Exists(dir))
								Directory.CreateDirectory(dir);

							assembly.Write(tempDllPath, new WriterParameters
							{
								WriteSymbols = hasPdb,
								SymbolStream = symbolStream,
								SymbolWriterProvider = new PdbWriterProvider()
							});
						}
						finally
						{
							symbolStream?.Dispose();
						}

						Console.WriteLine($"Migrated assembly to '{destination}'.");
					}
					else
					{
						hasPdb = false;

						if (verbose)
							Console.WriteLine($"Skipped assembly '{source}' due to lack of support types.");
					}
				}

				if (requiresSave)
				{
					File.Copy(tempDllPath, destination, true);
					File.Delete(tempDllPath);
					if (hasPdb)
					{
						File.Copy(tempPdbPath, destPdbPath, true);
						File.Delete(tempPdbPath);
					}
				}
			}

			return result;
		}

		private MigrationResult MigrateAssembly(AssemblyDefinition assembly, bool verbose)
		{
			var result = MigrateNetTypes(assembly, verbose);

			if (result.HasFlag(MigrationResult.PotentialJni))
			{
				result |= MigrateJniStrings(assembly, verbose);
			}

			return result;
		}

		private MigrationResult MigrateNetTypes(AssemblyDefinition assembly, bool verbose)
		{
			var result = MigrationResult.Skipped;

			foreach (var support in assembly.MainModule.GetTypeReferences())
			{
				if (!Mapping.TryGetAndroidXType(support.FullName, out var androidx) || support.FullName == androidx.FullName)
				{
					if (!result.HasFlag(MigrationResult.PotentialJni))
					{
						if (support.FullName == "Android.Runtime.RegisterAttribute")
							result |= MigrationResult.PotentialJni;
					}

					continue;
				}

				if (verbose)
					Console.WriteLine($"  Processing type reference '{support.FullName}'...");

				var old = support.FullName;
				support.Namespace = androidx.Namespace;

				if (verbose)
					Console.WriteLine($"    Mapped type '{old}' to '{support.FullName}'.");

				if (!string.IsNullOrWhiteSpace(androidx.Assembly) && support.Scope.Name != androidx.Assembly)
				{
					if (verbose)
						Console.WriteLine($"    Mapped assembly '{support.Scope.Name}' to '{androidx.Assembly}'.");

					support.Scope.Name = androidx.Assembly;
				}
				else if (support.Scope.Name == androidx.Assembly)
				{
					if (verbose)
						Console.WriteLine($"    Already mapped assembly '{support.Scope.Name}'.");
				}
				else
				{
					Console.WriteLine($"    *** Potential error for assembly {support.Scope.Name}' to '{androidx.Assembly}'. ***");
				}

				result |= MigrationResult.ContainedSupport;
			}

			return result;
		}

		private static MigrationResult MigrateJniStrings(AssemblyDefinition assembly, bool verbose)
		{
			Console.WriteLine($"    *** Assembly contains JNI strings. ***");

			return MigrationResult.Skipped;
		}
	}
}
