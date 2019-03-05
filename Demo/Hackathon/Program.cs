using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hackathon
{
	class Program
	{
		private static Dictionary<string, string> assemblyMappings = new Dictionary<string, string>
		{
			{ "Xamarin.Android.Support.v7.AppCompat", "Xamarin.AndroidX.Appcompat.Appcompat" },
		};

		static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : string.Empty;
			var migrated = args.Length > 1 ? args[1] : filename;

			var csv = LoadMapping("mappings.csv");

			var hasPdb = File.Exists(Path.ChangeExtension(filename, "pdb"));

			var readerParams = new ReaderParameters
			{
				ReadSymbols = hasPdb,
			};
			var assembly = AssemblyDefinition.ReadAssembly(filename, readerParams);

			var needsMigration = false;

			foreach (var module in assembly.Modules)
			{
				foreach (var type in module.Types)
				{
					var baseType = type.BaseType;
					if (baseType != null && csv.TryGetValue(baseType.FullName, out var newName))
					{
						if (assemblyMappings.TryGetValue(baseType.Scope.Name, out var newScope))
						{
							needsMigration = true;

							baseType.Namespace = newName.NS;
							baseType.Scope.Name = assemblyMappings[baseType.Scope.Name];
						}
						else
						{
							Console.WriteLine($" => Interesting scope: {baseType.Scope.Name} for type: {baseType.FullName}.");
						}
					}
				}
			}

			if (needsMigration)
			{
				var parameters = new WriterParameters
				{
					WriteSymbols = hasPdb
				};

				assembly.Write(migrated + ".temp", parameters);
				assembly.Dispose();

				File.Delete(migrated);

				if (hasPdb)
				{
					File.Delete(Path.ChangeExtension(migrated, "pdb"));
					File.Move(migrated + ".temp", migrated);
					File.Move(migrated + ".pdb", Path.ChangeExtension(migrated, "pdb"));
				}

				Console.WriteLine(" => Migrated assembly to: " + migrated);
			}
		}

		private static Dictionary<string, (string NS, string T)> LoadMapping(string csvFile)
		{
			var root = Path.GetDirectoryName(typeof(Program).Assembly.Location);

			var dic = new Dictionary<string, (string NS, string T)>();

			foreach (var line in File.ReadAllText(Path.Combine(root, csvFile)).Split('\r', '\n'))
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				var split = line.Split(',');

				var support = split[(int)Columns.AndroidSupportClassFullyQualified];
				var ns = split[(int)Columns.ManagedNamespaceXamarinAndroidX];
				var androidx = split[(int)Columns.AndroidXClassFullyQualified];

				if (string.IsNullOrWhiteSpace(support) ||
					string.IsNullOrWhiteSpace(support) ||
					string.IsNullOrWhiteSpace(androidx))
					continue;

				var t = androidx.Substring(ns.Length + 1);
				dic[support] = (NS: ns, T: t);
			}

			return dic;
		}

		private enum Columns
		{
			ClassName,
			AndroidSupportClass,
			AndroidXClass,
			AndroidSupportClassFullyQualified,
			AndroidXClassFullyQualified,
			PackageAndroidSupport,
			PackageAndroidX,
			ManagedNamespaceXamarinAndroidSupport,
			ManagedNamespaceXamarinAndroidX
		}
	}
}
