using Mono.Cecil;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class ManagedMigrationTests : BaseTests
	{
		[Theory]
		[InlineData(ManagedSupportDll, true)]
		[InlineData(BindingSupportDll, true)]
		[InlineData(MergedSupportDll, false)]
		public void AssembliesHaveSupportReferences(string assembly, bool hasSupportReference)
		{
			using (var support = AssemblyDefinition.ReadAssembly(assembly))
			{
				var types = support.MainModule.GetTypeReferences();
				var supportReferences = types.Where(t => t.FullName.StartsWith("Android.Support.") || t.FullName.StartsWith("Android.Arch."));
				if (hasSupportReference)
					Assert.NotEmpty(supportReferences);
				else
					Assert.Empty(supportReferences);
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll)]
		[InlineData(BindingSupportDll, BindingAndroidXDll)]
		public void AssembliesHaveTheSameNumberOfReferences(string supportDll, string androidXDll)
		{
			using (var support = AssemblyDefinition.ReadAssembly(supportDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				var supportReferences = support.MainModule.AssemblyReferences.ToArray();
				var androidxReferences = androidx.MainModule.AssemblyReferences.ToArray();

				Assert.Equal(supportReferences.Length, androidxReferences.Length);
				CecilAssert.NotEqual(supportReferences, androidxReferences);
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll)]
		[InlineData(BindingSupportDll, BindingAndroidXDll)]
		public void AssembliesHaveTheSameNumberOfTypes(string supportDll, string androidXDll)
		{
			using (var support = AssemblyDefinition.ReadAssembly(supportDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				var supportTypes = support.GetPublicTypes().ToArray();
				var androidxTypes = androidx.GetPublicTypes().ToArray();

				Assert.Equal(supportTypes.Length, androidxTypes.Length);
				CecilAssert.NotEqual(supportTypes, androidxTypes);
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		public void AssembliesHaveTheSameTypesAfterMigration(string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(androidx.GetPublicTypes(), migrated.GetPublicTypes());
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		public void AssembliesHaveTheSameReferencesAfterMigration(string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(
					androidx.MainModule.AssemblyReferences,
					migrated.MainModule.AssemblyReferences);
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		public void AllTypesHaveTheSameMembers(string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				var mTypes = migrated.GetPublicTypes().ToArray();
				var xTypes = androidx.GetPublicTypes().ToArray();

				for (var i = 0; i < xTypes.Length; i++)
				{
					CecilAssert.Equal(xTypes[i], mTypes[i]);
				}
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, "library_project_imports/res/layout/managedsupportlayout.xml", CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		[InlineData(BindingSupportDll, "library_project_imports/res/layout/supportlayout.xml", CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni | CecilMigrationResult.PotentialJavaArtifacts | CecilMigrationResult.ContainedJavaArtifacts)]
		public void EmbeddedJavaArtifactsAreMigrated(string supportDll, string layoutFile, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			{
				var aarResource = migrated.MainModule.Resources.FirstOrDefault(r => r.Name == "__AndroidLibraryProjects__.zip") as EmbeddedResource;
				using (var aarStream = aarResource.GetResourceStream())
				{
					var migratedLayout = ReadAarEntry(aarStream, layoutFile);

					Assert.Equal(
						"androidx.appcompat.widget.AppCompatButton",
						XDocument.Load(migratedLayout).Root.Elements().FirstOrDefault().Name.LocalName);
				}
			}
		}
	}
}
