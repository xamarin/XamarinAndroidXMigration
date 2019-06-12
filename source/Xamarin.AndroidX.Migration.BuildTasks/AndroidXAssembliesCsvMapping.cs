using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.AndroidX.Migration.BuildTasks
{
	public class AndroidXAssembliesCsvMapping : CsvMapping
	{
		private const string MappingResourcePath = "Tools/Mappings/androidx-assemblies.csv";

		private readonly SortedDictionary<string, string> assemblyMapping = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly SortedDictionary<string, string> packageMapping = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public AndroidXAssembliesCsvMapping()
		{
			var assembly = typeof(AndroidXTypesCsvMapping).Assembly;
			var mappingFile = Path.Combine(Path.GetDirectoryName(assembly.Location), MappingResourcePath);
			LoadMapping(mappingFile);
		}

		public AndroidXAssembliesCsvMapping(string mappingFile)
			: base(mappingFile)
		{
		}

		public AndroidXAssembliesCsvMapping(Stream csv)
			: base(csv)
		{
		}

		protected override void OnLoadRecord(string[] record)
		{
			if (record.Length < (int)Column.AndroidXNuGet)
				return;

			var supportAssembly = record[(int)Column.SupportNetAssembly];
			var xAssembly = record[(int)Column.AndroidXNetAssembly];
			var xPackage = record[(int)Column.AndroidXNuGet];

			if (string.IsNullOrWhiteSpace(supportAssembly) ||
				string.IsNullOrWhiteSpace(xAssembly) ||
				string.IsNullOrWhiteSpace(xPackage))
				return;

			assemblyMapping[supportAssembly] = xAssembly;
			packageMapping[supportAssembly] = xPackage;
		}

		public bool TryGetAndroidXAssembly(string supportAssembly, out string xAssembly) =>
			assemblyMapping.TryGetValue(supportAssembly, out xAssembly);

		public bool TryGetAndroidXPackage(string supportAssembly, out string xPackage) =>
			packageMapping.TryGetValue(supportAssembly, out xPackage);

		public bool ContainsSupportAssembly(string supportAssembly) =>
			assemblyMapping.ContainsKey(supportAssembly);

		public enum Column
		{
			SupportNetAssembly,
			AndroidXNetAssembly,
			SupportNuGet,
			AndroidXNuGet
		}
	}
}
