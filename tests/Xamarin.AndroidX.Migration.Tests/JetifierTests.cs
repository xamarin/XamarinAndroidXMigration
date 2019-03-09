using System.Linq;
using System.Xml.Linq;
using Xamarin.Android.Tools.Bytecode;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class JetifierTests : BaseTests
	{
		[Fact]
		public void CanReadAarFileWithDifferentSlashes()
		{
			var layout = ReadAarEntry(SupportAar, "res\\layout\\supportlayout.xml");

			Assert.NotNull(layout);
			Assert.True(layout.Length > 0);
		}

		[Fact]
		public void CanReadAarFile()
		{
			var layout = ReadAarEntry(SupportAar, "res/layout/supportlayout.xml");

			Assert.NotNull(layout);
			Assert.True(layout.Length > 0);
		}

		[Fact]
		public void LayoutFilesAreAsExpected()
		{
			var supportLayout = ReadAarEntry(SupportAar, "res/layout/supportlayout.xml");
			var androidxLayout = ReadAarEntry(AndroidXAar, "res/layout/supportlayout.xml");

			Assert.Equal(
				"android.support.v7.widget.AppCompatButton",
				XDocument.Load(supportLayout).Root.Elements().FirstOrDefault().Name.LocalName);

			Assert.Equal(
				"androidx.appcompat.widget.AppCompatButton",
				XDocument.Load(androidxLayout).Root.Elements().FirstOrDefault().Name.LocalName);
		}

		[Fact]
		public void JetifierMigratesLayoutFiles()
		{
			var migratedAar = SupportAar;
			// TODO: migratedAar = Jetifier.Jetify(SupportAar)

			var migratedLayout = ReadAarEntry(migratedAar, "res/layout/supportlayout.xml");
			var androidxLayout = ReadAarEntry(AndroidXAar, "res/layout/supportlayout.xml");

			Assert.Equal(
				"androidx.appcompat.widget.AppCompatButton",
				XDocument.Load(migratedLayout).Root.Elements().FirstOrDefault().Name.LocalName);

			Assert.Equal(
				"androidx.appcompat.widget.AppCompatButton",
				XDocument.Load(androidxLayout).Root.Elements().FirstOrDefault().Name.LocalName);
		}

		[Fact]
		public void CanReadJarFileAfterMigration()
		{
			var migratedAar = SupportAar;
			// TODO: migratedAar = Jetifier.Jetify(SupportAar)

			var jar = ReadAarEntry(migratedAar, "classes.jar");

			var classPath = new ClassPath();
			classPath.Load(jar);
			var packages = classPath.GetPackages();

			Assert.True(packages.Count > 0);
			Assert.Equal("com.xamarin.aarxercise", packages.Keys.FirstOrDefault());

			var classes = packages["com.xamarin.aarxercise"];

			Assert.True(classes.Count > 0);
		}

		[Fact]
		public void JavaTypesAreMigratedAfterJetifier()
		{
			var migratedAar = SupportAar;
			// TODO: migratedAar = Jetifier.Jetify(SupportAar)

			var jar = ReadAarEntry(migratedAar, "classes.jar");

			var classPath = new ClassPath();
			classPath.Load(jar);
			var packages = classPath.GetPackages();

			Assert.True(packages.Count > 0);
			Assert.Equal("com.xamarin.aarxercise", packages.Keys.FirstOrDefault());

			var classes = packages["com.xamarin.aarxercise"];
			var simpleFragment = classes.FirstOrDefault(c => c.ThisClass.Name.Value == "com/xamarin/aarxercise/SimpleFragment");

			Assert.Equal("androidx/fragment/app/Fragment", simpleFragment.SuperClass.Name.Value);
		}
	}
}
