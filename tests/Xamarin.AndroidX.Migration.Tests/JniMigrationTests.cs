using System;
using System.IO;
using System.Linq;
using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class JniMigrationTests : BaseTests
	{
		[Theory]
		[CecilfyData("", "")]
		[CecilfyData("()V", "()V")]
		[CecilfyData(
			"(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			"(Landroid/content/Context;)Landroidx/fragment/app/Fragment;")]
		[CecilfyData(
			"CreateFragment.(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			"CreateFragment.(Landroid/content/Context;)Landroidx/fragment/app/Fragment;")]
		[CecilfyData(
			"(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			"(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;")]
		[CecilfyData(
			"CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			"CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;")]
		[CecilfyData(
			"(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			"(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V")]
		[CecilfyData(
			"UpdateFragment.(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			"UpdateFragment.(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V")]
		[CecilfyData(
			"(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			"(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V")]
		[CecilfyData(
			"UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			"UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V")]
		[CecilfyData(
			"([Landroid/support/v4/graphics/PathParser;[Landroid/support/v4/graphics/PathParser;)V",
			"([Landroidx/core/graphics/PathParser;[Landroidx/core/graphics/PathParser;)V")]
		[CecilfyData(
			"([Landroid/support/v4/graphics/PathParser$PathDataNode;[Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
			"([Landroidx/core/graphics/PathParser$PathDataNode;[Landroidx/core/graphics/PathParser$PathDataNode;)V")]
		[CecilfyData(
			"([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;",
			"([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;")]
		[CecilfyData(
			"([Ljava/lang/Object;[[Landroid/support/v4/graphics/PathParser;)[Ljava/lang/Object;",
			"([Ljava/lang/Object;[[Landroidx/core/graphics/PathParser;)[Ljava/lang/Object;")]
		[CecilfyData(
			"java/lang/Object",
			"java/lang/Object")]
		[CecilfyData(
			"android/support/v4/app/Fragment",
			"androidx/fragment/app/Fragment")]
		[CecilfyData(
			"android/support/v7/app/ActionBar$Tab",
			"androidx/appcompat/app/ActionBar$Tab")]
		[CecilfyData(
			"android/support/v7/app/ActionBarDrawerToggle$Delegate",
			"androidx/appcompat/app/ActionBarDrawerToggle$Delegate")]
		[CecilfyData(
			"android/support/v7/app/ActionBar$Tab$ThisDoesNotExist",
			"androidx/appcompat/app/ActionBar$Tab$ThisDoesNotExist")]
		[CecilfyData(
			"android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist",
			"androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist")]
		[CecilfyData(
			"android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis",
			"androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis")]
		[CecilfyData(
			"(I)Landroid/support/v7/app/AlertDialog$Builder;",
			"(I)Landroidx/appcompat/app/AlertDialog$Builder;")]
		[CecilfyData(
			"(L;L;L;)Landroid/support/v7/app/AlertDialog$Builder;",
			"(L;L;L;)Landroidx/appcompat/app/AlertDialog$Builder;")]
		public void JniStringAreCorrectlyMapped(AvailableMigrators migrator, string supportJni, string androidxJni)
		{
			if (migrator == AvailableMigrators.CecilMigrator)
			{
				var cecilMigrator = new CecilMigrator();
				var wasChanged = cecilMigrator.MigrateJniString(supportJni, out var mappedJni);

				Assert.Equal(supportJni != androidxJni, wasChanged);
				if (wasChanged)
					Assert.Equal(androidxJni, mappedJni);
				else
					Assert.Null(mappedJni);
			}
			else if (migrator == AvailableMigrators.AndroidXMigrator)
			{
				var androidXMigrator = new AndroidXMigrator(null, null);
				var mapped = androidXMigrator.ReplaceJniSignatureRedth(supportJni);

				Assert.Equal(androidxJni, mapped);
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(migrator));
			}
		}

		[Theory]
		[CecilfyData(ManagedSupportDll, CecilMigrationResult.ContainedSupport)]
		[CecilfyData(BindingSupportDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		[CecilfyData(MergedSupportDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void MigrationDoesNotThrow(AvailableMigrators migrator, string assembly, CecilMigrationResult expectedResult)
		{
			var mappedDll = RunMigration(migrator, assembly, expectedResult);

			Assert.True(File.Exists(mappedDll));
		}

		[Theory]
		[CecilfyData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void RegisterAttributesOnMethodsAreMappedCorrectly(AvailableMigrators migrator, string supportDll, string androidxDll, CecilMigrationResult expectedResult)
		{
			var mappedDll = RunMigration(migrator, supportDll, expectedResult);

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
		[CecilfyData(BindingSupportDll, BindingAndroidXDll, CecilMigrationResult.ContainedSupport | CecilMigrationResult.PotentialJni | CecilMigrationResult.ContainedJni)]
		public void InstructionsInMethodsAreMappedCorrectly(AvailableMigrators migrator, string supportDll, string androidxDll, CecilMigrationResult expectedResult)
		{
			var mappedDll = RunMigration(migrator, supportDll, expectedResult);

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

		[Theory]
		[InlineData(MergedAndroidXDll)]
		public void EnsureAllMethodWithJniHaveRegisterAttributes(string assemblyPath)
		{
			// this is not really a test for any reason but to confirm the
			// logic in the migration:
			//  - all methods with register attributes belong to a type with a
			//    register attribute
			//  - all methods that contain JNI belong to a method that has a
			//    register attribute, or, if there is no register attribute on
			//    the method, then the type has it.
			using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath))
			{
				foreach (var type in assembly.MainModule.Types)
				{
					var typeRegister = type.GetRegisterAttribute();
					var annotationAttribute = type.GetAnnotationAttribute();

					foreach (var property in type.Properties)
					{
						var propertyRegister = property.GetRegisterAttribute();

						// make sure if the property has a register attribute, then so does the type
						if (propertyRegister != null)
							if ((typeRegister ?? annotationAttribute) == null)
								throw new Exception($"Attribute missing on type {type.FullName} for property {property.Name}.");
					}

					foreach (var method in type.Methods)
					{
						var methodRegister = method.GetRegisterAttribute();

						var property = type.Properties.FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);
						var propertyRegister = property?.GetRegisterAttribute();

						// make sure if the method has a register attribute, then so does the type
						if (methodRegister != null)
							if (typeRegister == null)
								throw new Exception($"Attribute missing on type {type.FullName} for method {method.Name}.");

						if (!method.HasBody)
							continue;

						foreach (var instruction in method.Body.Instructions)
						{
							if (instruction.OpCode == OpCodes.Ldstr)
							{
								var str = instruction.Operand as string;

								// make sure if the method contains JNI, then either the type
								// or method has a [Register] attribute
								if ((str.Contains("/") && !str.Contains("//")) ||
									(str.Contains("(") && str.Contains(")")))
								{
									if (!IsTypeException(type))
										if (typeRegister == null)
											throw new Exception($"Attribute missing on type {type.FullName} for JNI in method {method.Name}.");

									if (!IsMethodException(method))
										if ((methodRegister ?? propertyRegister) == null)
											throw new Exception($"Attribute missing on JNI method & property {method.Name} for type {type.FullName}.");
								}
							}
						}
					}
				}
			}

			bool IsTypeException(TypeDefinition type)
			{
				return (
					// the __TypeRegistrations type does not have register attributes
					type.Name.EndsWith("__TypeRegistrations")
				);
			}

			bool IsMethodException(MethodDefinition method)
			{
				return (
					// the __TypeRegistrations type does not have register attributes
					method.DeclaringType.Name.EndsWith("__TypeRegistrations")
				) || (
					// the static constructor does not have an attribute
					method.Name == ".cctor"
				) || (
					// the invokers/implementors do not have register attributes on the methods
					method.DeclaringType.IsClass &&
					!method.DeclaringType.IsPublic &&
					(method.DeclaringType.Name.EndsWith("Invoker") || method.DeclaringType.Name.EndsWith("Implementor"))
				);
			}
		}
	}
}
