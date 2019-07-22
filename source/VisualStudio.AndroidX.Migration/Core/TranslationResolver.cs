using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VisualStudio.AndroidX.Migration
{
	public class TranslationResolver : ITranslationResolver
	{
		int SupportNamespace = 0;
		int SupportTypeName = 1;
		int AndroidXNamespace = 2;
		int AndroidXTypeName = 3;

		int SupportNuget = 2;
		int AndroidXNuget = 3;
		int AndroidXVersion = 4;

		public TranslationResolver(IList<string> androidAssemblyLocations, IList<string> androidXAssemblyLocations)
		{
			Namespaces = new Dictionary<string, string>();
			FullTypeNames = new Dictionary<string, string>();
			Nugets = new Dictionary<string, KeyValuePair<string, string>>();

			var conversionFile = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Resources\\androidx-mapping.csv");
			foreach (var line in File.ReadLines(conversionFile).Skip(1))
			{
				var values = line.Split(',');
				if (values[SupportNamespace] != string.Empty && values[AndroidXNamespace] != string.Empty)
				{ 
					if (!Namespaces.Any(n => n.Key == values[SupportNamespace]))
						Namespaces.Add(values[SupportNamespace], values[AndroidXNamespace]);

					if (!FullTypeNames.Any(t => t.Key == $"{values[SupportNamespace]}.{values[SupportTypeName]}"))
						FullTypeNames.Add($"{values[SupportNamespace]}.{values[SupportTypeName]}", $"{values[AndroidXNamespace]}.{values[AndroidXTypeName]}");
				}

			}

			AndroidAssemblies = AddAssemblies(androidAssemblyLocations ?? new List<string>(), AndroidAssemblies);
			AndroidXAssemblies = AddAssemblies(androidXAssemblyLocations ?? new List<string>(), AndroidXAssemblies);

			var nugetsFile = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Resources\\androidx-assemblies.csv");
			foreach (var line in File.ReadLines(nugetsFile).Skip(1))
			{
				var values = line.Split(',');
				if (!Nugets.Any(n => n.Key == values[SupportNuget]))
					Nugets.Add(values[SupportNuget], new KeyValuePair<string, string>(values[AndroidXNuget], values[AndroidXVersion]));
			}
		}

		public IList<Assembly> AddAssemblies(IList<string> assemblyLocations, IList<Assembly> assemblies)
		{
			assemblies = assemblies ?? new List<Assembly>();
			foreach (var assembly in assemblyLocations)
				assemblies.Add(Assembly.ReflectionOnlyLoadFrom(assembly));

			return assemblies;
		}

		public IList<Assembly> AndroidAssemblies { get; private set; }

		public IList<Assembly> AndroidXAssemblies { get; private set; }

		public Dictionary<string, string> Namespaces { get; private set; }

		public Dictionary<string, string> FullTypeNames { get; private set; }

		public Dictionary<string, KeyValuePair<string, string>> Nugets { get; private set; }
	}

	public interface ITranslationResolver
	{
		IList<Assembly> AndroidAssemblies { get; }
		IList<Assembly> AndroidXAssemblies { get; }

		Dictionary<string, string> Namespaces { get; }
		Dictionary<string, string> FullTypeNames { get; }

		Dictionary<string, KeyValuePair<string, string>> Nugets { get; }
	}
}
