using Mono.Cecil;
using System.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Cecil.Tests
{
	public class JniMigrationTests : BaseTests
	{
		[Theory]
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
		public void JniStringAreCorrectlyMapped(string supportJni, string androidxJni)
		{
			var mappedJni = supportJni;
			// TODO: mappedJni = JniMapper.Map(supportJni)

			Assert.Equal(androidxJni, mappedJni);
		}

		[Theory]
		[InlineData(BindingSupportDll, BindingAndroidXDll)]
		public void RegisterAttributesOnMethodsAreMappedCorrectly(string supportDll, string androidxDll)
		{
			var mappedDll = supportDll;
			// TODO mappedDll = JniMapper.Map(supportDll)

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
						Assert.Equal(
							xMethod.GetRegisterAttribute().GetArguments(),
							mMethod.GetRegisterAttribute().GetArguments());
					}
				}
			}
		}
	}
}
