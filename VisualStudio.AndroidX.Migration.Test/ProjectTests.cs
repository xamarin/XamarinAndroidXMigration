using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace VisualStudio.AndroidX.Migration
{
	public class ProjectTests
	{
		[Fact]
		public void when_nuget_version_is_not_inlined_replace_it()
		{
			var csproj = versionedCsProj;

			var resolver = new TranslationResolver(new List<string>(), new List<string> { });
			var projectFixer = new ProjectRewriter(resolver, new NullProgress());

			csproj = projectFixer.RewriteCSProj(csproj);

			Assert.Contains(@"<PackageReference Include=""Xamarin.Google.Android.Material"">", csproj);
			Assert.DoesNotContain(@"<Version>28.0.0.1</Version>", csproj);
			Assert.Contains(@"<Version>1.0.0-preview02</Version>", csproj); //replace version number
			Assert.Contains(@"<Version>27.0.0.1</Version>", csproj); //don't remove version for xamarin.essentials
		}
		
        [Fact]
        public void when_include_migration_then_migration_is_present()
        {
            var csproj = sampleCsProj;

            var resolver = new TranslationResolver(new List<string>(), new List<string> { });
            var projectFixer = new ProjectRewriter(resolver, new NullProgress());

            csproj = projectFixer.RewriteCSProj(csproj, true);

            Assert.Contains(@"<PackageReference Include=""Xamarin.AndroidX.Migration"" Version=""1.0.0-preview03"" />", csproj);
        }

        [Fact]
		public void when_nuget_is_àppcompat_replace_with_androidx()
		{
			var csproj = sampleCsProj;

			var resolver = new TranslationResolver(new List<string>(), new List<string> { });
			var projectFixer = new ProjectRewriter(resolver, new NullProgress());

			csproj = projectFixer.RewriteCSProj(csproj);

			Assert.Contains(@"<PackageReference Include=""Xamarin.AndroidX.Core"" Version=""1.2.0.2"" />", csproj);
		}


		[Fact]
		public void when_version_is_inline_replace_inline()
		{
			var csproj = sampleCsProj;

			var resolver = new TranslationResolver(new List<string>(), new List<string> { });
			var projectFixer = new ProjectRewriter(resolver, new NullProgress());

			csproj = projectFixer.RewriteCSProj(csproj);

			Assert.Contains(@"<PackageReference Include=""Xamarin.AndroidX.Browser"" Version=""1.2.0.2"" />", csproj);
		}

		[Fact]
		public void in_poolmath_replace_androidx()
		{
			var csproj = poolMathCsproj;

			var resolver = new TranslationResolver(new List<string>(), new List<string> { });
			var projectFixer = new ProjectRewriter(resolver, new NullProgress());

			csproj = projectFixer.RewriteCSProj(csproj);

			Assert.Contains(@"<PackageReference Include=""Xamarin.AndroidX.Core"">
      <Version>1.2.0.2</Version>
    </PackageReference>", csproj);
		}

		[Fact]
		public void when_nuget_is_support_replace_with_androidx()
		{
			var csproj = sampleCsProj;

			var resolver = new TranslationResolver(new List<string>(), new List<string> { });
			var projectFixer = new ProjectRewriter(resolver, new NullProgress());
				
			csproj = projectFixer.RewriteCSProj(csproj);

			Assert.Contains(@"<PackageReference Include=""Xamarin.Google.Android.Material"" Version=""1.1.0.2-rc3"" />", csproj);
		}

		string sampleCsProj =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{EA94CF80-A750-4881-9468-638D0472C6C9}</ProjectGuid>
    <ProjectTypeGuids>{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <TemplateGuid>{84dd83c5-0fe3-4294-9419-09e7c8ba324f}</TemplateGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>App43</RootNamespace>
    <AssemblyName>App43</AssemblyName>
    <FileAlignment>512</FileAlignment>
    <AndroidApplication>True</AndroidApplication>
    <AndroidResgenFile>Resources\Resource.designer.cs</AndroidResgenFile>
    <AndroidResgenClass>Resource</AndroidResgenClass>
    <GenerateSerializationAssemblies>Off</GenerateSerializationAssemblies>
    <AndroidUseLatestPlatformSdk>false</AndroidUseLatestPlatformSdk>
    <TargetFrameworkVersion>v9.0</TargetFrameworkVersion>
    <AndroidManifest>Properties\AndroidManifest.xml</AndroidManifest>
    <MonoAndroidResourcePrefix>Resources</MonoAndroidResourcePrefix>
    <MonoAndroidAssetsPrefix>Assets</MonoAndroidAssetsPrefix>
    <AndroidEnableSGenConcurrent>true</AndroidEnableSGenConcurrent>
    <AndroidHttpClientHandlerType>Xamarin.Android.Net.AndroidClientHandler</AndroidHttpClientHandlerType>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>True</DebugSymbols>
    <DebugType>portable</DebugType>
    <Optimize>False</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidUseSharedRuntime>True</AndroidUseSharedRuntime>
    <AndroidLinkMode>None</AndroidLinkMode>
    <EmbedAssembliesIntoApk>False</EmbedAssembliesIntoApk>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugSymbols>True</DebugSymbols>
    <DebugType>portable</DebugType>
    <Optimize>True</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidManagedSymbols>true</AndroidManagedSymbols>
    <AndroidUseSharedRuntime>False</AndroidUseSharedRuntime>
    <AndroidLinkMode>SdkOnly</AndroidLinkMode>
    <EmbedAssembliesIntoApk>True</EmbedAssembliesIntoApk>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Xml"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""Mono.Android"" />
    <Reference Include=""System.Numerics"" />
    <Reference Include=""System.Numerics.Vectors"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""MainActivity.cs"" />
    <Compile Include=""Resources\Resource.designer.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""Resources\AboutResources.txt"" />
    <None Include=""Properties\AndroidManifest.xml"" />
    <None Include=""Assets\AboutAssets.txt"" />
  </ItemGroup>
  <ItemGroup>
    <AndroidResource Include=""Resources\layout\activity_main.axml"">
      <SubType>Designer</SubType>
    </AndroidResource>
    <AndroidResource Include=""Resources\layout\content_main.axml"">
          <SubType>Designer</SubType>
    </AndroidResource>
    <AndroidResource Include=""Resources\values\colors.xml"" />
    <AndroidResource Include=""Resources\values\dimens.xml"" />
    <AndroidResource Include=""Resources\values\ic_launcher_background.xml"" />
    <AndroidResource Include=""Resources\values\strings.xml"" />
    <AndroidResource Include=""Resources\values\styles.xml"" />
    <AndroidResource Include=""Resources\menu\menu_main.xml"" />
    <AndroidResource Include=""Resources\mipmap-anydpi-v26\ic_launcher.xml"" />
    <AndroidResource Include=""Resources\mipmap-anydpi-v26\ic_launcher_round.xml"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher_round.png"" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""Xamarin.Android.Support.Design"" Version=""28.0.0.1"" />
    <PackageReference Include=""Xamarin.Android.Support.Core.Utils"" Version=""28.0.0.1"" />
    <PackageReference Include=""Xamarin.Android.Support.CustomTabs"" Version=""28.0.0.1"" />
    <PackageReference Include=""Xamarin.Essentials"" Version=""1.1.0"" />
	<PackageReference Include=""Xamarin.Android.Support.Compat"" />
  </ItemGroup>
  <Import Project=""$(MSBuildExtensionsPath)\Xamarin\Android\Xamarin.Android.CSharp.targets"" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it.
    Other similar extension points exist, see Microsoft.Common.targets.
    <Target Name=""BeforeBuild"">
    </Target>
    <Target Name=""AfterBuild"">
    </Target>
  -->
</Project>";


		string versionedCsProj =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{EA94CF80-A750-4881-9468-638D0472C6C9}</ProjectGuid>
    <ProjectTypeGuids>{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <TemplateGuid>{84dd83c5-0fe3-4294-9419-09e7c8ba324f}</TemplateGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>App43</RootNamespace>
    <AssemblyName>App43</AssemblyName>
    <FileAlignment>512</FileAlignment>
    <AndroidApplication>True</AndroidApplication>
    <AndroidResgenFile>Resources\Resource.designer.cs</AndroidResgenFile>
    <AndroidResgenClass>Resource</AndroidResgenClass>
    <GenerateSerializationAssemblies>Off</GenerateSerializationAssemblies>
    <AndroidUseLatestPlatformSdk>false</AndroidUseLatestPlatformSdk>
    <TargetFrameworkVersion>v9.0</TargetFrameworkVersion>
    <AndroidManifest>Properties\AndroidManifest.xml</AndroidManifest>
    <MonoAndroidResourcePrefix>Resources</MonoAndroidResourcePrefix>
    <MonoAndroidAssetsPrefix>Assets</MonoAndroidAssetsPrefix>
    <AndroidEnableSGenConcurrent>true</AndroidEnableSGenConcurrent>
    <AndroidHttpClientHandlerType>Xamarin.Android.Net.AndroidClientHandler</AndroidHttpClientHandlerType>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>True</DebugSymbols>
    <DebugType>portable</DebugType>
    <Optimize>False</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidUseSharedRuntime>True</AndroidUseSharedRuntime>
    <AndroidLinkMode>None</AndroidLinkMode>
    <EmbedAssembliesIntoApk>False</EmbedAssembliesIntoApk>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugSymbols>True</DebugSymbols>
    <DebugType>portable</DebugType>
    <Optimize>True</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidManagedSymbols>true</AndroidManagedSymbols>
    <AndroidUseSharedRuntime>False</AndroidUseSharedRuntime>
    <AndroidLinkMode>SdkOnly</AndroidLinkMode>
    <EmbedAssembliesIntoApk>True</EmbedAssembliesIntoApk>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Xml"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""Mono.Android"" />
    <Reference Include=""System.Numerics"" />
    <Reference Include=""System.Numerics.Vectors"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""MainActivity.cs"" />
    <Compile Include=""Resources\Resource.designer.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""Resources\AboutResources.txt"" />
    <None Include=""Properties\AndroidManifest.xml"" />
    <None Include=""Assets\AboutAssets.txt"" />
  </ItemGroup>
  <ItemGroup>
    <AndroidResource Include=""Resources\layout\activity_main.axml"">
      <SubType>Designer</SubType>
    </AndroidResource>
    <AndroidResource Include=""Resources\layout\content_main.axml"">
          <SubType>Designer</SubType>
    </AndroidResource>
    <AndroidResource Include=""Resources\values\colors.xml"" />
    <AndroidResource Include=""Resources\values\dimens.xml"" />
    <AndroidResource Include=""Resources\values\ic_launcher_background.xml"" />
    <AndroidResource Include=""Resources\values\strings.xml"" />
    <AndroidResource Include=""Resources\values\styles.xml"" />
    <AndroidResource Include=""Resources\menu\menu_main.xml"" />
    <AndroidResource Include=""Resources\mipmap-anydpi-v26\ic_launcher.xml"" />
    <AndroidResource Include=""Resources\mipmap-anydpi-v26\ic_launcher_round.xml"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-hdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-mdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xhdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xxhdpi\ic_launcher_round.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher_foreground.png"" />
    <AndroidResource Include=""Resources\mipmap-xxxhdpi\ic_launcher_round.png"" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""Xamarin.Android.Support.Design"" >
		<Version>28.0.0.1</Version>
	</PackageReference>
    <PackageReference Include=""Xamarin.Essentials"" Version=""1.1.0"" >
		<Version>27.0.0.1</Version>
	</PackageReference>
  </ItemGroup>
  <Import Project=""$(MSBuildExtensionsPath)\Xamarin\Android\Xamarin.Android.CSharp.targets"" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it.
    Other similar extension points exist, see Microsoft.Common.targets.
    <Target Name=""BeforeBuild"">
    </Target>
    <Target Name=""AfterBuild"">
    </Target>
  -->
</Project>";

		string poolMathCsproj = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{C62B0A31-9EF4-46F9-A4FA-2CCDA1937444}</ProjectGuid>
    <ProjectTypeGuids>{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <RootNamespace>TroubleFreePool.Droid</RootNamespace>
    <AssemblyName>com.troublefreepool.poolmath</AssemblyName>
    <TargetFrameworkVersion>v9.0</TargetFrameworkVersion>
    <AndroidApplication>True</AndroidApplication>
    <AndroidResgenFile>Resources\Resource.designer.cs</AndroidResgenFile>
    <AndroidResgenClass>Resource</AndroidResgenClass>
    <AndroidManifest>Properties\AndroidManifest.xml</AndroidManifest>
    <MonoAndroidResourcePrefix>Resources</MonoAndroidResourcePrefix>
    <MonoAndroidAssetsPrefix>Assets</MonoAndroidAssetsPrefix>
    <AndroidUseSharedRuntime>false</AndroidUseSharedRuntime>
    <AndroidTlsProvider>
    </AndroidTlsProvider>
    <AndroidLinkMode>None</AndroidLinkMode>
    <AndroidUseAapt2>true</AndroidUseAapt2>
    <AndroidEnableMultiDex>true</AndroidEnableMultiDex>
    <AndroidHttpClientHandlerType>Xamarin.Android.Net.AndroidClientHandler</AndroidHttpClientHandlerType>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug</OutputPath>
    <DefineConstants>DEBUG;</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <EmbedAssembliesIntoApk>false</EmbedAssembliesIntoApk>
    <AndroidSupportedAbis>arm64-v8a;armeabi-v7a;x86;x86_64</AndroidSupportedAbis>
    <AndroidUseSharedRuntime>true</AndroidUseSharedRuntime>
    <AndroidKeyStore>
    </AndroidKeyStore>
    <AndroidSigningKeyStore>..\Signing\poolmath.keystore</AndroidSigningKeyStore>
    <AndroidSigningStorePass>poolmath</AndroidSigningStorePass>
    <AndroidSigningKeyAlias>poolmath</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>poolmath</AndroidSigningKeyPass>
    <EnableProguard>true</EnableProguard>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release</OutputPath>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidManagedSymbols>true</AndroidManagedSymbols>
    <AndroidKeyStore>True</AndroidKeyStore>
    <AndroidSigningKeyStore>..\Signing\poolmath.keystore</AndroidSigningKeyStore>
    <AndroidSigningStorePass>poolmath</AndroidSigningStorePass>
    <AndroidSigningKeyAlias>poolmath</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>poolmath</AndroidSigningKeyPass>
    <AndroidSupportedAbis>armeabi-v7a;arm64-v8a</AndroidSupportedAbis>
    <AndroidLinkMode>SdkOnly</AndroidLinkMode>
    <AotAssemblies>true</AotAssemblies>
    <JavaMaximumHeapSize>1G</JavaMaximumHeapSize>
    <JavaOptions>-Xss4096k</JavaOptions>
    <EnableLLVM>true</EnableLLVM>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'AppStore|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release</OutputPath>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <AndroidManagedSymbols>true</AndroidManagedSymbols>
    <AndroidKeyStore>True</AndroidKeyStore>
    <AndroidSigningKeyStore>..\Signing\poolmath.keystore</AndroidSigningKeyStore>
    <AndroidSigningStorePass>poolmath</AndroidSigningStorePass>
    <AndroidSigningKeyAlias>poolmath</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>poolmath</AndroidSigningKeyPass>
    <AndroidSupportedAbis>armeabi-v7a;arm64-v8a</AndroidSupportedAbis>
    <AndroidLinkMode>SdkOnly</AndroidLinkMode>
    <AotAssemblies>true</AotAssemblies>
    <JavaMaximumHeapSize>1G</JavaMaximumHeapSize>
    <JavaOptions>-Xss4096k</JavaOptions>
    <EnableLLVM>true</EnableLLVM>
    <DefineConstants>APPSTORE;</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition="" $(SolutionDir) == '' "">
    <SolutionDir>$(MSBuildThisFileDirectory)..\</SolutionDir>
  </PropertyGroup>
  <PropertyGroup>
    <AndroidSdkBuildToolsVersion>26.0.2</AndroidSdkBuildToolsVersion>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Xml"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""Mono.Android"" />
    <Reference Include=""System.Net.Http"" />
    <Reference Include=""Mono.Android.Export"" />
    <Reference Include=""System.IO.Compression"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Effects\RoundTheCornersEffectAndroid.cs"" />
    <Compile Include=""MainActivity.cs"" />
    <Compile Include=""Renderers\BindablePickerRendererDroid.cs"" />
    <Compile Include=""Renderers\DatePickerRenderer.cs"" />
    <Compile Include=""Renderers\HumbleEntryRendererAndroid.cs"" />
    <Compile Include=""Resources\Resource.designer.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
    <Compile Include=""MainApplication.cs"" />
    <Compile Include=""Renderers\MenuViewCellRenderer.cs"" />
    <Compile Include=""Renderers\SelectAllEntryRenderer.cs"" />
    <Compile Include=""Renderers\ChemicalOverviewRenderer.cs"" />
    <Compile Include=""Services\AccentColorService.cs"" />
    <Compile Include=""Services\SubscriptionService.cs"" />
    <Compile Include=""SplashActivity.cs"" />
    <Compile Include=""Renderers\CardContentViewRendererDroid.cs"" />
    <Compile Include=""Renderers\SelectAllExtendedEntryRenderer.cs"" />
    <Compile Include=""AuthProvider\FacebookAuthProvider.cs"" />
    <Compile Include=""AuthProvider\GoogleAuthProvider.cs"" />
    <Compile Include=""Renderers\NavigationViewRenderer.cs"" />
    <Compile Include=""Services\ExportFileService.cs"" />
    <Compile Include=""Services\HudService.cs"" />
    <Compile Include=""Renderers\ChemicalOutlineFrameRenderer.cs"" />
  </ItemGroup>
  <ItemGroup>
    <GoogleServicesJson Include=""google-services.json"" />
    <None Include=""Resources\AboutResources.txt"" />
    <None Include=""Properties\AndroidManifest.xml"" />
    <None Include=""Assets\AboutAssets.txt"" />
  </ItemGroup>
  <ItemGroup>
    <AndroidResource Include=""Resources\drawable\loginbg.png"" />
  </ItemGroup>
  <ItemGroup>
    <ProguardConfiguration Include=""proguard\proguard.cfg"" />
    <ProguardConfiguration Include=""proguard\proguard-gps.txt"" />
    <ProguardConfiguration Include=""proguard.txt"" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""AndHUD"">
      <Version>1.4.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Annotations"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v7.CardView"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Core.Utils"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Core.UI"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Compat"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Design"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.Fragment"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v7.AppCompat"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v7.MediaRouter"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v7.RecyclerView"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v7.Palette"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Android.Support.v4"">
      <Version>28.0.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.GooglePlayServices.Auth"">
      <Version>60.1142.1</Version>
    </PackageReference>
    <PackageReference Include=""Newtonsoft.Json"">
      <Version>12.0.1</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.AppCenter"">
      <Version>1.14.0</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.AppCenter.Analytics"">
      <Version>1.14.0</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.AppCenter.Crashes"">
      <Version>1.14.0</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.AppCenter.Distribute"">
      <Version>1.14.0</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.AppCenter.Push"">
      <Version>1.14.0</Version>
    </PackageReference>
    <PackageReference Include=""MonkeyCache"">
      <Version>1.2.0-beta</Version>
    </PackageReference>
    <PackageReference Include=""MonkeyCache.FileStore"">
      <Version>1.2.0-beta</Version>
    </PackageReference>
    <PackageReference Include=""NETStandard.Library"">
      <Version>2.0.3</Version>
    </PackageReference>
    <PackageReference Include=""Plugin.InAppBilling"">
      <Version>2.1.0.187-beta</Version>
    </PackageReference>
    <PackageReference Include=""Plugin.Permissions"">
      <Version>3.0.0.12</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Build.Download"">
      <Version>0.4.12-preview3</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Facebook.Android"">
      <Version>4.38.0</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Forms"">
      <Version>3.6.0.344457</Version>
    </PackageReference>
    <PackageReference Include=""Humanizer.Core"">
      <Version>2.5.16</Version>
    </PackageReference>
    <PackageReference Include=""Xamarin.Essentials"">
      <Version>1.1.0</Version>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\PoolMath.Core\PoolMath.Core.csproj"">
      <Project>{E0A1FD19-6E12-449D-8C07-5A00EB73C465}</Project>
      <Name>PoolMath.Core</Name>
    </ProjectReference>
    <ProjectReference Include=""..\PoolMathApp.Share\PoolMathApp.Share.csproj"">
      <Project>{C6C52109-A9B0-4027-A9A3-F1D2A3314F0E}</Project>
      <Name>PoolMathApp.Share</Name>
    </ProjectReference>
    <ProjectReference Include=""..\AmazonIap\AmazonIap.csproj"">
      <Project>{B986F10C-01CB-4F0A-881D-29C06A91DF89}</Project>
      <Name>AmazonIap</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project=""$(MSBuildExtensionsPath)\Xamarin\Android\Xamarin.Android.CSharp.targets"" />
</Project>
";
	}
}
