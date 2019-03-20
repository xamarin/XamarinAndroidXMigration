using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using Xamarin.AndroidX.Migration.Cecil;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class JniMigrationTests : BaseTests
	{
		[Theory]
		[InlineData("", "")]
		[InlineData("()V", "()V")]
		[InlineData(
			"(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			"(Landroid/content/Context;)Landroidx/fragment/app/Fragment;")]
		[InlineData(
			"CreateFragment.(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			"CreateFragment.(Landroid/content/Context;)Landroidx/fragment/app/Fragment;")]
		[InlineData(
			"(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			"(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;")]
		[InlineData(
			"CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			"CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;")]
		[InlineData(
			"(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			"(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V")]
		[InlineData(
			"UpdateFragment.(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			"UpdateFragment.(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V")]
		[InlineData(
			"(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			"(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V")]
		[InlineData(
			"UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			"UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V")]
		[InlineData(
			"([Landroid/support/v4/graphics/PathParser;[Landroid/support/v4/graphics/PathParser;)V",
			"([Landroidx/core/graphics/PathParser;[Landroidx/core/graphics/PathParser;)V")]
		[InlineData(
			"([Landroid/support/v4/graphics/PathParser$PathDataNode;[Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
			"([Landroidx/core/graphics/PathParser$PathDataNode;[Landroidx/core/graphics/PathParser$PathDataNode;)V")]
		[InlineData(
			"([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;",
			"([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;")]
		[InlineData(
			"([Ljava/lang/Object;[[Landroid/support/v4/graphics/PathParser;)[Ljava/lang/Object;",
			"([Ljava/lang/Object;[[Landroidx/core/graphics/PathParser;)[Ljava/lang/Object;")]
		[InlineData(
			"java/lang/Object",
			"java/lang/Object")]
		[InlineData(
			"android/support/v4/app/Fragment",
			"androidx/fragment/app/Fragment")]
		[InlineData(
			"android/support/v7/app/ActionBar$Tab",
			"androidx/appcompat/app/ActionBar$Tab")]
		[InlineData(
			"android/support/v7/app/ActionBarDrawerToggle$Delegate",
			"androidx/appcompat/app/ActionBarDrawerToggle$Delegate")]
		[InlineData(
			"android/support/v7/app/ActionBar$Tab$ThisDoesNotExist",
			"androidx/appcompat/app/ActionBar$Tab$ThisDoesNotExist")]
		[InlineData(
			"android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist",
			"androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist")]
		[InlineData(
			"android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis",
			"androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis")]
		[InlineData(
			"(I)Landroid/support/v7/app/AlertDialog$Builder;",
			"(I)Landroidx/appcompat/app/AlertDialog$Builder;")]
		[InlineData(
			"(L;L;L;)Landroid/support/v7/app/AlertDialog$Builder;",
			"(L;L;L;)Landroidx/appcompat/app/AlertDialog$Builder;")]
		public void JniStringAreCorrectlyMapped(string supportJni, string androidxJni)
		{
			var wasChanged = migrator.MigrateJniString(supportJni, out var mappedJni);
			if (wasChanged)
				Assert.Equal(androidxJni, mappedJni);
			else
				Assert.Null(mappedJni);
		}

		[Theory]
		[InlineData(ManagedSupportDll)]
		[InlineData(BindingSupportDll)]
		[InlineData(MergedSupportDll)]
		public void MigrationDoesNotThrow(string assembly)
		{
			var mappedDll = Utils.GetTempFilename();

			var result = migrator.Migrate(assembly, mappedDll);

			Assert.NotEqual(MigrationResult.Skipped, result);
		}

		[Theory]
		[InlineData(BindingSupportDll, BindingAndroidXDll)]
		public void RegisterAttributesOnMethodsAreMappedCorrectly(string supportDll, string androidxDll)
		{
			var mappedDll = Utils.GetTempFilename();

			var result = migrator.Migrate(supportDll, mappedDll);

			var expectedResult =
				MigrationResult.ContainedJni |
				MigrationResult.ContainedSupport |
				MigrationResult.PotentialJni;
			Assert.Equal(expectedResult, result);

			using (var support = AssemblyDefinition.ReadAssembly(supportDll))
			using (var mapped = AssemblyDefinition.ReadAssembly(mappedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidxDll))
			{
				var supportTypes = support.MainModule.GetTypes().ToArray();
				var mappedTypes = mapped.MainModule.GetTypes().ToArray();
				var androidxTypes = androidx.MainModule.GetTypes().ToArray();

				for (int i = 0; i < supportTypes.Length; i++)
				{
					var sType = supportTypes[i];
					var mType = mappedTypes[i];
					var xType = androidxTypes[i];

					// make sure the types have the same register attributes before and after the migration
					Assert.Equal(
						sType.GetRegisterAttribute().GetArguments(),
						mType.GetRegisterAttribute().GetArguments());
					Assert.Equal(
						sType.GetRegisterAttribute().GetArguments(),
						xType.GetRegisterAttribute().GetArguments());

					for (int j = 0; j < sType.Methods.Count; j++)
					{
						var mMethod = mType.Methods[j];
						var xMethod = xType.Methods[j];

						// make sure all the member register attributes have been migrated
						// skip the last parameter as that is the generated handler name
						Assert.Equal(
							xMethod.GetRegisterAttribute().GetArguments().Take(2),
							mMethod.GetRegisterAttribute().GetArguments().Take(2));
					}
				}
			}
		}

		[Theory]
		[InlineData(BindingSupportDll, BindingAndroidXDll)]
		public void InstructionsInMethodsAreMappedCorrectly(string supportDll, string androidxDll)
		{
			var mappedDll = Utils.GetTempFilename();

			migrator.Migrate(supportDll, mappedDll);

			using (var support = AssemblyDefinition.ReadAssembly(supportDll))
			using (var mapped = AssemblyDefinition.ReadAssembly(mappedDll))
			using (var androidx = AssemblyDefinition.ReadAssembly(androidxDll))
			{
				var supportTypes = support.MainModule.GetTypes().ToArray();
				var mappedTypes = mapped.MainModule.GetTypes().ToArray();
				var androidxTypes = androidx.MainModule.GetTypes().ToArray();

				for (int i = 0; i < supportTypes.Length; i++)
				{
					var sType = supportTypes[i];
					var mType = mappedTypes[i];
					var xType = androidxTypes[i];

					for (int j = 0; j < sType.Methods.Count; j++)
					{
						var sMethod = sType.Methods[j];
						var mMethod = mType.Methods[j];
						var xMethod = xType.Methods[j];

						if (sMethod.HasBody)
						{
							for (int k = 0; k < sMethod.Body.Instructions.Count; k++)
							{
								var sInstruction = sMethod.Body.Instructions[k];
								var mInstruction = mMethod.Body.Instructions[k];
								var xInstruction = xMethod.Body.Instructions[k];

								if (sInstruction.OpCode == OpCodes.Ldstr &&
									sInstruction.Operand is string sOperand &&
									(sOperand.Contains("android/support") || sOperand.Contains("android/arch")))
								{
									var mOperand = mInstruction.Operand as string;
									var xOperand = xInstruction.Operand as string;

									Assert.Equal(xOperand, mOperand);
								}
							}
						}
					}
				}
			}
		}
	}
}
