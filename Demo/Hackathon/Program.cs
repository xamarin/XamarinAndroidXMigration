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
			{ "Xamarin.Android.Support.v7.AppCompat", "Xamarin.AndroidX.AppCompat" },
			{ "Xamarin.Android.Support.Fragment", "Xamarin.AndroidX.Fragment" },
			{ "Xamarin.Android.Support.Compat", "Xamarin.AndroidX.Core" },
			{ "Xamarin.Android.Support.Core.UI", "Xamarin.AndroidX.Legacy.Support.Core.UI" },
			{ "Xamarin.Android.Support.Design", "Xamarin.Google.Android.Material" },
			{ "Xamarin.Android.Support.v7.CardView", "Xamarin.AndroidX.CardView" },
			{ "Xamarin.Android.Support.v7.RecyclerView", "Xamarin.AndroidX.RecyclerView" },
			{ "Xamarin.Android.Support.DrawerLayout", "Xamarin.AndroidX.DrawerLayout" },
			{ "Xamarin.Android.Support.ViewPager", "Xamarin.AndroidX.ViewPager" },
			{ "Xamarin.Android.Support.SwipeRefreshLayout", "Xamarin.AndroidX.SwipeRefreshLayout" },
			{ "Xamarin.Android.Support.CoordinaterLayout", "Xamarin.AndroidX.CoordinatorLayout" },
		};

		static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : string.Empty;
			var migrated = args.Length > 1 ? args[1] : Path.ChangeExtension(filename, "migrated.dll");

			var csv = LoadMapping("mappings.csv");

			var hasPdb = File.Exists(Path.ChangeExtension(filename, "pdb"));

			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(Path.GetDirectoryName(filename));
			var readerParams = new ReaderParameters
			{
				ReadSymbols = hasPdb,
				AssemblyResolver = resolver
			};
			var assembly = AssemblyDefinition.ReadAssembly(filename, readerParams);
			//Console.WriteLine($"Processing assembly '{filename}'...");

			var needsMigration = false;
			foreach (var module in assembly.Modules)
			{
				foreach (var typeRef in module.GetTypeReferences())
				{
					if (!csv.TryGetValue(typeRef.FullName, out var newName) || typeRef.FullName == newName.FN)
						continue;

					Console.WriteLine($" => Processing type reference '{typeRef.FullName}'...");

					var old = typeRef.FullName;
					typeRef.Namespace = newName.NS;
					Console.WriteLine($"     => Mapped type '{old}' to '{typeRef.FullName}'.");

					if (assemblyMappings.TryGetValue(typeRef.Scope.Name, out var newAssembly) && typeRef.Scope.Name != newAssembly)
					{
						Console.WriteLine($"     => Mapped assembly '{typeRef.Scope.Name}' to '{newAssembly}'.");
						typeRef.Scope.Name = newAssembly;
					}
					else if (assemblyMappings.ContainsValue(typeRef.Scope.Name))
					{
						Console.WriteLine($"     => Already mapped assembly '{typeRef.Scope.Name}'.");
					}
					else
					{
						Console.WriteLine($"     *** Potential error for assembly {typeRef.Scope.Name}' ***");
					}

					needsMigration = true;
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

				Console.WriteLine($"Migrated assembly to '{migrated}'.");
			}
		}

		private static Dictionary<string, (string NS, string T, string FN)> LoadMapping(string csvFile)
		{
			var root = Path.GetDirectoryName(typeof(Program).Assembly.Location);

			var dic = new Dictionary<string, (string NS, string T, string FN)>();

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
				dic[support] = (NS: ns, T: t, FN: androidx);
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
