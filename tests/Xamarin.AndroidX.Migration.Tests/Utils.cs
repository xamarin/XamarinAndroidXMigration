using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Xamarin.AndroidX.Migration.Tests
{
	public static class Utils
	{
		private const string RegisterAttributeFullName = "Android.Runtime.RegisterAttribute";
		private const string AnnotationAttributeFullName = "Android.Runtime.AnnotationAttribute";

		public static string GetTempFilename(string filename = null) =>
			Path.Combine(
				Path.GetTempPath(),
				Guid.NewGuid().ToString(),
				filename ?? Guid.NewGuid().ToString());

		public static IEnumerable<TypeDefinition> GetPublicTypes(this AssemblyDefinition assembly) =>
			assembly.MainModule.GetTypes().Where(t => t.IsPublic || t.IsNestedPublic);

		public static CustomAttribute GetAnnotationAttribute(this TypeDefinition type) =>
			type.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == AnnotationAttributeFullName);

		public static CustomAttribute GetRegisterAttribute(this TypeDefinition type) =>
			type.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == RegisterAttributeFullName);

		public static CustomAttribute GetRegisterAttribute(this MethodDefinition method) =>
			method.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == RegisterAttributeFullName);

		public static CustomAttribute GetRegisterAttribute(this PropertyDefinition property) =>
			property.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == RegisterAttributeFullName);

		public static object[] GetArguments(this CustomAttribute attribute) =>
			attribute?.ConstructorArguments?.Select(a => a.Value)?.ToArray() ?? new object[0];

		public static async Task DownloadFileAsync(string facebookTestUrl, string facebookZip)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
					"Xamarin.AndroidX.Migration.Tests",
					"1.0.0"));

				using (var stream = await client.GetStreamAsync(facebookTestUrl))
				using (var dest = File.OpenWrite(facebookZip))
				{
					await stream.CopyToAsync(dest);
				}
			}
		}
	}
}
