using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

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

		public static string RunMigration(AvailableMigrators migrator, string supportDll, CecilMigrationResult expectedResult)
		{
			if (migrator == AvailableMigrators.CecilMigrator)
				return RunCecilMigratorMigration(supportDll, expectedResult);
			if (migrator == AvailableMigrators.AndroidXMigrator)
				return RunAndroidXMigratorMigration(supportDll);

			throw new ArgumentOutOfRangeException(nameof(migrator));
		}

		public static string RunCecilMigratorMigration(string supportDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = GetTempFilename();

			var migrator = new CecilMigrator();
			var result = migrator.Migrate(supportDll, migratedDll);

			// TODO: implement the JNI migration
			if (false)
			{
				Assert.Equal(expectedResult, result);
			}

			return migratedDll;
		}

		public static string RunAndroidXMigratorMigration(string supportDll)
		{
			var migratedDll = GetTempFilename();

			var dir = Path.GetDirectoryName(migratedDll);
			Directory.CreateDirectory(dir);

			var migrator = new AndroidXMigrator(supportDll, migratedDll);
			migrator.Migrate();

			return $"{migratedDll}.redth.dll";
		}
	}

	public enum AvailableMigrators
	{
		CecilMigrator,
		AndroidXMigrator,
	}
}
