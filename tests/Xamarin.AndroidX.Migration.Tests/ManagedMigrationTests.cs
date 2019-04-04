using Mono.Cecil;
using System.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class ManagedMigrationTests : BaseTests
	{
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
		[CecilfyData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport)]
		[CecilfyData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void AssembliesHaveTheSameTypesAfterMigration(AvailableMigrators migrator, string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(migrator, supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(androidx.GetPublicTypes(), migrated.GetPublicTypes());
			}
		}

		[Theory]
		[CecilfyData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport)]
		[CecilfyData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void AssembliesHaveTheSameReferencesAfterMigration(AvailableMigrators migrator, string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(migrator, supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(
					androidx.MainModule.AssemblyReferences,
					migrated.MainModule.AssemblyReferences);
			}
		}

		[Theory]
		[CecilfyData(ManagedSupportDll, ManagedAndroidXDll, CecilMigrationResult.ContainedSupport)]
		[CecilfyData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void AllTypesHaveTheSameMembers(AvailableMigrators migrator, string supportDll, string androidXDll, CecilMigrationResult expectedResult)
		{
			var migratedDll = RunMigration(migrator, supportDll, expectedResult);

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
	}
}
