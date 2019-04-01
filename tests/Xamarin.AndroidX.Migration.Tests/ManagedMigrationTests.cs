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
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		public void AssembliesHaveTheSameTypesAfterMigration(string supportDll, string androidXDll, AvailableMigrators migrator, CecilMigrationResult expectedResult)
		{
			var migratedDll = Utils.RunMigration(migrator, supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(androidx.GetPublicTypes(), migrated.GetPublicTypes());
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		public void AssembliesHaveTheSameReferencesAfterMigration(string supportDll, string androidXDll, AvailableMigrators migrator, CecilMigrationResult expectedResult)
		{
			var migratedDll = Utils.RunMigration(migrator, supportDll, expectedResult);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(
					androidx.MainModule.AssemblyReferences,
					migrated.MainModule.AssemblyReferences);
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.CecilMigrator, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, AvailableMigrators.AndroidXMigrator, CecilMigrationResult.Skipped)]
		public void AllTypesHaveTheSameMembers(string supportDll, string androidXDll, AvailableMigrators migrator, CecilMigrationResult expectedResult)
		{
			var migratedDll = Utils.RunMigration(migrator, supportDll, expectedResult);

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
