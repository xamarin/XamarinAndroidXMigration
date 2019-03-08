using System;
using System.Collections.Generic;
using System.IO;

namespace Hackathon
{
	public class CsvMapping
	{
		private readonly SortedDictionary<string, FullType> mapping = new SortedDictionary<string, FullType>();
		private readonly SortedDictionary<string, FullType> reverseMapping = new SortedDictionary<string, FullType>();

		public CsvMapping(string filename)
		{
			Filename = filename ?? throw new ArgumentNullException(nameof(filename));
			LoadMapping(Filename);
		}

		public string Filename { get; }

		private void LoadMapping(string path)
		{
			mapping.Clear();

			foreach (var line in File.ReadAllText(path).Split('\r', '\n'))
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

				if (string.IsNullOrWhiteSpace(supportNamespace) ||
					string.IsNullOrWhiteSpace(supportType) ||
					string.IsNullOrWhiteSpace(xNamespace) ||
					string.IsNullOrWhiteSpace(xType) ||
					string.IsNullOrWhiteSpace(xAssembly))
					continue;

				var support = new FullType(supportNamespace, supportType);
				var androidX = new FullType(xAssembly, xNamespace, xType);

				mapping[support.FullName] = androidX;
				reverseMapping[androidX.FullName] = support;
			}
		}

		public bool TryGetAndroidXType(string supportFullName, out FullType androidxType) =>
			mapping.TryGetValue(supportFullName, out androidxType);

		public bool TryGetSupportType(string androidxFullName, out FullType supportType) =>
			reverseMapping.TryGetValue(androidxFullName, out supportType);

		public bool ContainsSupportType(string supportFullName) =>
			mapping.ContainsKey(supportFullName);

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
