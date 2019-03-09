using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Migration.Cecil.Tests
{
	public static class Utils
	{
		public static string GetTempFilename(string filename = null) =>
			Path.Combine(
				Path.GetTempPath(),
				Guid.NewGuid().ToString(),
				filename ?? Guid.NewGuid().ToString());

		public static IEnumerable<TypeDefinition> GetPublicTypes(this AssemblyDefinition assembly) =>
			assembly.MainModule.GetTypes().Where(t => t.IsPublic || t.IsNestedPublic);
	}
}
