using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.AndroidX.Migration.Cecil
{
	public class CsvMapping
	{
		private static readonly Lazy<CsvMapping> instance = new Lazy<CsvMapping>(() => new CsvMapping());

		private const string MappingResource = "Xamarin.AndroidX.Migration.Cecil.androidx-mapping.csv";

		private readonly SortedDictionary<string, FullType> mapping = new SortedDictionary<string, FullType>();
		private readonly SortedDictionary<string, FullType> reverseMapping = new SortedDictionary<string, FullType>();
		private readonly SortedDictionary<string, FullType> javaMapping = new SortedDictionary<string, FullType>();

		public static CsvMapping Instance => instance.Value;

		public CsvMapping()
		{
			var assembly = typeof(CsvMapping).Assembly;

			using (var stream = assembly.GetManifestResourceStream(MappingResource))
			using (var reader = new StreamReader(stream))
			{
				LoadMapping(reader);
			}
		}

		public CsvMapping(Stream csv)
		{
			using (var reader = new StreamReader(csv))
			{
				LoadMapping(reader);
			}
		}

		private void LoadMapping(TextReader reader)
		{
			mapping.Clear();

			foreach (var line in reader.ReadToEnd().Split('\r', '\n'))
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				var split = line.Split(',');

				if (split.Length < (int)Column.Messages)
					continue;

				var supportNamespace = split[(int)Column.SupportNetNamespace];
				var supportType = split[(int)Column.SupportNetType];
				var xNamespace = split[(int)Column.AndroidXNetNamespace];
				var xType = split[(int)Column.AndroidXNetType];

				var xAssembly = split[(int)Column.AndroidXNetAssembly];

				var supportPackage = split[(int)Column.SupportJavaPackage];
				var supportClass = split[(int)Column.SupportJavaClass];
				var xPackage = split[(int)Column.AndroidXJavaPackage];
				var xClass = split[(int)Column.AndroidXJavaClass];

				if (string.IsNullOrWhiteSpace(supportNamespace) ||
					string.IsNullOrWhiteSpace(supportType) ||
					string.IsNullOrWhiteSpace(xNamespace) ||
					string.IsNullOrWhiteSpace(xType) ||
					string.IsNullOrWhiteSpace(xAssembly) ||
					string.IsNullOrWhiteSpace(supportPackage) ||
					string.IsNullOrWhiteSpace(supportClass) ||
					string.IsNullOrWhiteSpace(xPackage) ||
					string.IsNullOrWhiteSpace(xClass))
					continue;

				var support = new FullType(supportNamespace, supportType);
				var androidX = new FullType(xAssembly, xNamespace, xType);
				var supportJava = new FullType(supportPackage, supportClass);
				var androidXJava = new FullType(xPackage, xClass);

				mapping[support.FullName] = androidX;
				reverseMapping[androidX.FullName] = support;
				javaMapping[supportJava.JavaFullName] = androidXJava;
			}
		}

		public bool TryGetAndroidXType(string supportFullName, out FullType androidxType) =>
			mapping.TryGetValue(supportFullName, out androidxType);

		public bool TryGetAndroidXClass(string supportJavaFullName, out FullType androidxClass) =>
			javaMapping.TryGetValue(supportJavaFullName, out androidxClass);

		public bool TryGetSupportType(string androidxFullName, out FullType supportType) =>
			reverseMapping.TryGetValue(androidxFullName, out supportType);

		public bool ContainsSupportType(string supportFullName) =>
			mapping.ContainsKey(supportFullName);

		public bool ContainsSupportClass(string supportJavaFullName) =>
			javaMapping.ContainsKey(supportJavaFullName);

		public bool ContainsAndroidXType(string androidxFullName) =>
			reverseMapping.ContainsKey(androidxFullName);

		public enum Column
		{
			SupportNetNamespace,
			SupportNetType,
			AndroidXNetNamespace,
			AndroidXNetType,
			AndroidXNetAssembly,
			SupportJavaPackage,
			SupportJavaClass,
			AndroidXJavaPackage,
			AndroidXJavaClass,
			Messages
		}
	}
}
