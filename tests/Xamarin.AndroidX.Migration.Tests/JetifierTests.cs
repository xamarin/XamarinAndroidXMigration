using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

		[Fact]
		public async Task JetifierWrapperMigratesFacebookAar()
		{
			var facebookFilename = "facebook-android-sdk";
			var facebookVersion = "4.40.0";
			var facebookFullName = $"{facebookFilename}-{facebookVersion}";
			var facebookTestUrl = $"https://origincache.facebook.com/developers/resources/?id={facebookFullName}.zip";

			var workspace = Utils.GetTempFilename();
			Directory.CreateDirectory(workspace);

			// download facebook
			var facebookZip = Path.Combine(workspace, "facebook.zip");
			await Utils.DownloadFileAsync(facebookTestUrl, facebookZip);
			ZipFile.ExtractToDirectory(facebookZip, workspace);

			// run jetifier
			var aarFiles = Directory.GetFiles(workspace, "*.aar", SearchOption.AllDirectories);
			var results = RunJetifierWrapper(aarFiles);

			// read the classes
			var commonAar = results.FirstOrDefault(pait => Path.GetFileNameWithoutExtension(pait.Input) == "facebook-common");
			var jar = ReadAarEntry(commonAar.Output, "classes.jar");
			var classPath = new ClassPath();
			classPath.Load(jar);
			var packages = classPath.GetPackages();

			// read the type
			var classes = packages["com.facebook"];
			var activity = classes.FirstOrDefault(c => c.ThisClass.Name.Value == "com/facebook/FacebookActivity");

			// read the layout
			var migratedLayout = ReadAarEntry(commonAar.Output, "res/layout/com_facebook_device_auth_dialog_fragment.xml");

			// check
			Assert.Equal("androidx/fragment/app/FragmentActivity", activity.SuperClass.Name.Value);
			Assert.Equal(
				"androidx.cardview.widget.CardView",
				XDocument.Load(migratedLayout).Root.Name.LocalName);
		}
	}
}
