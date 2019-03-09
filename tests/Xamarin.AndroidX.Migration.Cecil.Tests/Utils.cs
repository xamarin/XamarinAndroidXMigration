using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Migration.Cecil.Tests
{
	public static class Utils
	{
		private const string RegisterAttributeFullName = "Android.Runtime.RegisterAttribute";

		public static string GetTempFilename(string filename = null) =>
			Path.Combine(
				Path.GetTempPath(),
				Guid.NewGuid().ToString(),
				filename ?? Guid.NewGuid().ToString());

		public static IEnumerable<TypeDefinition> GetPublicTypes(this AssemblyDefinition assembly) =>
			assembly.MainModule.GetTypes().Where(t => t.IsPublic || t.IsNestedPublic);

		public static CustomAttribute GetRegisterAttribute(this TypeDefinition type) =>
			type.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == RegisterAttributeFullName);

		public static CustomAttribute GetRegisterAttribute(this MethodDefinition method) =>
			method.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == RegisterAttributeFullName);

		public static object[] GetArguments(this CustomAttribute attribute) =>
			attribute?.ConstructorArguments?.Select(a => a.Value)?.ToArray() ?? new object[0];
	}
}
