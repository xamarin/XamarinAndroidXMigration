using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Android.Tools.Bytecode;

namespace Xamarin.AndroidX.Migration
{
	public class CecilMigrator
	{
		private const string RegisterAttributeFullName = "Android.Runtime.RegisterAttribute";
		private const string StringFullName = "System.String";

		public CecilMigrator()
			: this(CsvMapping.Instance)
		{
		}

		public CecilMigrator(Stream stream)
			: this(new CsvMapping(stream))
		{
		}

		public CecilMigrator(CsvMapping mapping)
		{
			Mapping = mapping;
		}

		public CsvMapping Mapping { get; }

		public bool Verbose { get; set; }

		public CecilMigrationResult Migrate(IEnumerable<MigrationPair> assemblies)
		{
			var result = CecilMigrationResult.Skipped;

			foreach (var pair in assemblies)
			{
				result |= Migrate(pair.Source, pair.Destination);
			}

			return result;
		}

		public CecilMigrationResult Migrate(MigrationPair assemblies) =>
			Migrate(assemblies.Source, assemblies.Destination);

		public CecilMigrationResult Migrate(string source, string destination)
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
			var result = CecilMigrationResult.Skipped;

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
					if (Verbose)
						Console.WriteLine($"Processing assembly '{source}'...");

					result = MigrateAssembly(assembly);

					requiresSave =
						result.HasFlag(CecilMigrationResult.ContainedSupport) ||
						result.HasFlag(CecilMigrationResult.ContainedJni);

					var dir = Path.GetDirectoryName(destination);
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					if (requiresSave)
					{
						Stream symbolStream = null;
						PdbWriterProvider symbolWriter = null;
						if (hasPdb)
						{
							symbolStream = File.Create(tempPdbPath);
							symbolWriter = new PdbWriterProvider();
						}

						try
						{
							assembly.Write(tempDllPath, new WriterParameters
							{
								WriteSymbols = hasPdb,
								SymbolStream = symbolStream,
								SymbolWriterProvider = symbolWriter
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
						if (Verbose)
							Console.WriteLine($"Skipped assembly '{source}' due to lack of support types.");

						if (!source.Equals(destination, StringComparison.OrdinalIgnoreCase))
						{
							if (Verbose)
								Console.WriteLine($"Copying source assembly '{source}' to '{destination}'.");

							File.Copy(source, destination, true);
							if (hasPdb)
								File.Copy(pdbPath, destPdbPath, true);
						}
					}
				}

				if (requiresSave)
				{
					if (File.Exists(tempDllPath))
					{
						File.Copy(tempDllPath, destination, true);
						File.Delete(tempDllPath);
					}
					if (File.Exists(tempPdbPath))
					{
						File.Copy(tempPdbPath, destPdbPath, true);
						File.Delete(tempPdbPath);
					}
				}
			}

			return result;
		}

		public bool MigrateJniString(string jniString, out string newJniString)
		{
			newJniString = null;
			return false;
		}

		private CecilMigrationResult MigrateAssembly(AssemblyDefinition assembly)
		{
			var result = MigrateNetTypes(assembly);

			if (result.HasFlag(CecilMigrationResult.PotentialJni))
			{
				result |= MigrateJniStrings(assembly);
			}

			return result;
		}

		private CecilMigrationResult MigrateNetTypes(AssemblyDefinition assembly)
		{
			var result = CecilMigrationResult.Skipped;

			foreach (var support in assembly.MainModule.GetTypeReferences())
			{
				if (!Mapping.TryGetAndroidXType(support.FullName, out var androidx) || support.FullName == androidx.FullName)
				{
					if (!result.HasFlag(CecilMigrationResult.PotentialJni))
					{
						if (support.FullName == RegisterAttributeFullName)
							result |= CecilMigrationResult.PotentialJni;
					}

					continue;
				}

				if (Verbose)
					Console.WriteLine($"  Processing type reference '{support.FullName}'...");

				var old = support.FullName;
				support.Namespace = androidx.Namespace;

				if (Verbose)
					Console.WriteLine($"    Mapped type '{old}' to '{support.FullName}'.");

				if (!string.IsNullOrWhiteSpace(androidx.Assembly) && support.Scope.Name != androidx.Assembly)
				{
					if (Verbose)
						Console.WriteLine($"    Mapped assembly '{support.Scope.Name}' to '{androidx.Assembly}'.");

					support.Scope.Name = androidx.Assembly;
				}
				else if (support.Scope.Name == androidx.Assembly)
				{
					if (Verbose)
						Console.WriteLine($"    Already mapped assembly '{support.Scope.Name}'.");
				}
				else
				{
					Console.WriteLine($"    *** Potential error for assembly {support.Scope.Name}' to '{androidx.Assembly}'. ***");
				}

				result |= CecilMigrationResult.ContainedSupport;
			}

			return result;
		}

		private CecilMigrationResult MigrateJniStrings(AssemblyDefinition assembly)
		{
			Console.WriteLine($"    *** Assembly contains JNI strings. ***");

			return CecilMigrationResult.Skipped;
		}
	}
}
