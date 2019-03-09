using Mono.Cecil;
using System.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Cecil.Tests
{
	public class ManagedMigrationTests : BaseTests
	{
		private readonly static Migrator migrator = new Migrator();

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
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, MigrationResult.Skipped)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, MigrationResult.PotentialJni)]
		public void AssembliesHaveTheSameTypesAfterMigration(string supportDll, string androidXDll, MigrationResult deltaResult)
		{
			var migratedDll = Utils.GetTempFilename();

			var result = migrator.Migrate(supportDll, migratedDll, false);

			Assert.Equal(MigrationResult.ContainedSupport | deltaResult, result);

			using (var migrated = AssemblyDefinition.ReadAssembly(migratedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidXDll))
			{
				CecilAssert.Equal(androidx.GetPublicTypes(), migrated.GetPublicTypes());
			}
		}

		[Theory]
		[InlineData(ManagedSupportDll, ManagedAndroidXDll, MigrationResult.Skipped)]
		[InlineData(BindingSupportDll, BindingAndroidXDll, MigrationResult.PotentialJni)]
		public void AllTypesHaveTheSameMembers(string supportDll, string androidXDll, MigrationResult deltaResult)
		{
			var migratedDll = Utils.GetTempFilename();

			var result = migrator.Migrate(supportDll, migratedDll, false);

			Assert.Equal(MigrationResult.ContainedSupport | deltaResult, result);

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
