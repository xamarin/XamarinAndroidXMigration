#tool "nuget:?package=xunit.runner.console&version=2.4.1"

#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"

var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta05";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";

var azureBuildNumber = "4945";
var azureBuildUrl = $"https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds/{azureBuildNumber}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";

var legacyBuildNumber = "4437";
var legacyBuildUrl = $"https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds/{legacyBuildNumber}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";

var NUGET_EXE = "./tools/nuget.exe";
if (!FileExists(NUGET_EXE)) {
    DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", NUGET_EXE);
}

var JAVA_HOME = EnvironmentVariable ("JAVA_HOME");

var BUILD_BASE_VERSION = EnvironmentVariable("BUILD_BASE_VERSION") ?? "1.0.0";
var BUILD_PREVIEW_LABEL = EnvironmentVariable("BUILD_PREVIEW_LABEL") ?? "preview";
var BUILD_NUMBER = EnvironmentVariable("BUILD_NUMBER") ?? "0";
var BUILD_PRERELEASE_OVERRIDE = EnvironmentVariable("BUILD_PRERELEASE_OVERRIDE") ?? "";
var BUILD_PRODUCE_PRERELEASE = bool.Parse(EnvironmentVariable("BUILD_PRODUCE_PRERELEASE") ?? "true");

var BUILD_VERSION_STABLE = $"{BUILD_BASE_VERSION}";
var BUILD_VERSION_PRERELEASE = string.IsNullOrEmpty(BUILD_PRERELEASE_OVERRIDE)
    ? $"{BUILD_BASE_VERSION}-{BUILD_PREVIEW_LABEL}.{BUILD_NUMBER}"
    : $"{BUILD_BASE_VERSION}-{BUILD_PRERELEASE_OVERRIDE}";

var BUILD_PACKAGE_VERSION = BUILD_PRODUCE_PRERELEASE
    ? BUILD_VERSION_PRERELEASE
    : BUILD_VERSION_STABLE;

Task("JetifierWrapper")
    .Does(() =>
{
    // download the jetifier
    if (!FileExists("./externals/jetifier.zip"))
        DownloadFile(jetifierDownloadUrl, "./externals/jetifier.zip");
    if (!DirectoryExists("./externals/jetifier-standalone") && !DirectoryExists("./externals/jetifier"))
        Unzip("./externals/jetifier.zip", "./externals");
    if (!DirectoryExists("./externals/jetifier") && DirectoryExists("./externals/jetifier-standalone"))
        MoveDirectory("./externals/jetifier-standalone", "./externals/jetifier");

    // setup
    var outputDir = MakeAbsolute((DirectoryPath)"./output/JetifierWrapper");
    var jetifierWrapperRoot = MakeAbsolute((DirectoryPath)"./source/Xamarin.AndroidX.Migration/jetifierWrapper");
    var jetifierWrapperJar = $"{outputDir}/JetifierWrapper.jar";
    EnsureDirectoryExists(outputDir);

    // javac
    var jarFiles = GetFiles("./externals/jetifier/lib/*.jar");
    var classPath = string.Join(System.IO.Path.PathSeparator.ToString(), jarFiles);
    var srcFiles = GetFiles($"{jetifierWrapperRoot}/src/**/*.java");
    var combinedSource = string.Join(" ", srcFiles);
    StartProcess("javac", $"-cp {classPath} {combinedSource}");

    // jar
    var srcRoot = $"{jetifierWrapperRoot}/src";
    var files = GetFiles($"{srcRoot}/**/*.class").ToList();
    files.Insert(0, $"{srcRoot}/META-INF/MANIFEST.MF");
    var combinedfiles = string.Join(" ", files.Select(f => f.FullPath.Replace(srcRoot, ".")));
    StartProcess("jar", new ProcessSettings {
        Arguments = $"cfm {jetifierWrapperJar} {combinedfiles}",
        WorkingDirectory = srcRoot
    });

    // zip
    CopyFiles(jarFiles, outputDir);
    Zip(outputDir, "./output/JetifierWrapper.zip");
});

Task("JavaProjects")
    .Does(() =>
{
    var nativeProjects = new [] {
        "tests/Xamarin.AndroidX.Migration/Aarxersise.Java.AndroidX",
        "tests/Xamarin.AndroidX.Migration/Aarxersise.Java.Support",
        "samples/com.xamarin.CoolLibrary",
    };

    foreach (var native in nativeProjects) {
        var abs = MakeAbsolute((DirectoryPath)native);
        if (IsRunningOnWindows()) {
            StartProcess($"{abs}/gradlew.bat", $"assembleDebug -p {abs}");
        } else {
            StartProcess("bash", $"{abs}/gradlew assembleDebug -p {abs}");
        }
    }
});

Task("DownloadNativeFacebookSdk")
    .Does(() =>
{
    var sdkRoot = "./externals/test-assets/facebook-sdk/";

    var facebookFilename = "facebook-android-sdk";
    var facebookVersion = "4.40.0";
    var facebookFullName = $"{facebookFilename}-{facebookVersion}";
    var facebookTestUrl = $"https://origincache.facebook.com/developers/resources/?id={facebookFullName}.zip";

    var zipName = $"{sdkRoot}{facebookFilename}.zip";

    EnsureDirectoryExists(sdkRoot);

    if (!FileExists(zipName)) {
        DownloadFile(facebookTestUrl, zipName);
        Unzip(zipName, sdkRoot);
    }
});

Task("DownloadAndroidXAssets")
    .Does(() =>
{
    var externalsRoot = "./externals/";
    EnsureDirectoryExists(externalsRoot);

    var dllsRoot = $"{externalsRoot}test-assets/merged-dlls/";
    EnsureDirectoryExists(dllsRoot);

    var zipName = $"{externalsRoot}AndroidX-NuGets.zip";
    if (!FileExists(zipName)) {
        DownloadFile(azureBuildUrl, zipName);
        Unzip(zipName, externalsRoot);

        CopyFileToDirectory($"{externalsRoot}nuget/AndroidSupport.Merged.dll", dllsRoot);
        CopyFileToDirectory($"{externalsRoot}nuget/AndroidX.Merged.dll", dllsRoot);
        CopyFileToDirectory($"{externalsRoot}nuget/androidx-mapping.csv", "mappings");
    }
});

Task("DownloadLegacyAssets")
    .Does(() =>
{
    var externalsRoot = "./externals/legacy/";
    EnsureDirectoryExists(externalsRoot);

    var zipName = $"{externalsRoot}Android-NuGets.zip";
    if (!FileExists(zipName)) {
        DownloadFile(legacyBuildUrl, zipName);
        Unzip(zipName, externalsRoot);
    }
});

Task("NativeAssets")
    .IsDependentOn("JavaProjects")
    .IsDependentOn("JetifierWrapper")
    .IsDependentOn("DownloadNativeFacebookSdk")
    .IsDependentOn("DownloadAndroidXAssets");

Task("Libraries")
    .IsDependentOn("JetifierWrapper")
    .IsDependentOn("DownloadAndroidXAssets")
    .IsDependentOn("NativeAssets")
    .IsDependentOn("VSTestPrepare")
    .Does(() =>
{
    // needed for nuget restore
    EnsureDirectoryExists("./output/nugets");

    MSBuild("Xamarin.AndroidX.Migration.sln", new MSBuildSettings {
        Configuration = configuration,
        Restore = true,
        MaxCpuCount = 0,
        Properties = {
            { "DesignTimeBuild", new [] { "false" } },
            { "AndroidSdkBuildToolsVersion", new [] { "28.0.3" } },
            { "JavaSdkDirectory", new [] { JAVA_HOME } },
        },
    });

    // copy the build tasks
    {
        var root = $"./source/Xamarin.AndroidX.Migration/BuildTasks/bin/{configuration}/net47";
        var outRoot = $"./output/Xamarin.AndroidX.Migration.BuildTasks";
        EnsureDirectoryExists($"{outRoot}/build/Tools/");
        CopyDirectory($"{root}/Tools/", $"{outRoot}/build/Tools/");
        CopyFiles($"{root}/Mono.*", $"{outRoot}/build/");
        CopyFiles($"{root}/Xamarin.*", $"{outRoot}/build/");
        Zip($"{outRoot}/", $"./output/Xamarin.AndroidX.Migration.BuildTasks.zip");
    }

    MSBuild("./source/VisualStudio.AndroidX.Migration/VisualStudio.AndroidX.Migration.sln", new MSBuildSettings {
        Configuration = configuration,
        Restore = true,
        BinaryLogger = new MSBuildBinaryLogSettings
        {
            Enabled = true,
            FileName = MakeAbsolute((DirectoryPath)"./output/vsbuild.binlog").FullPath,
        }
    });
});

Task("Tests")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    var testProjects = GetFiles("./tests/**/*.Tests.csproj");
    foreach (var proj in testProjects) {
        try {
            DotNetCoreTest(proj.GetFilename().ToString(), new DotNetCoreTestSettings {
                Configuration = configuration,
                NoBuild = true,
                TestAdapterPath = ".",
                Logger = "xunit",
                WorkingDirectory = proj.GetDirectory(),
                ResultsDirectory = $"./output/test-results/{proj.GetFilenameWithoutExtension()}",
            });
        } catch (Exception ex) {
            Error("Tests failed with an error.");
            Error(ex);
        }
    }
});

Task("VSTestPrepare")
    .IsDependentOn("DownloadAndroidXAssets")
    .IsDependentOn("DownloadLegacyAssets")
    .Does(() =>
{
    var externalRoot = "./externals/";
    var testAssembliesFolder = "./tests/VisualStudio.AndroidX.Migration/Test/Assemblies/";

    if (!DirectoryExists($"{testAssembliesFolder}AndroidX")) {
        var androidXNugets = new [] {
            $"{externalRoot}nuget/Xamarin.AndroidX.AppCompat.1*-preview.*.nupkg",
            $"{externalRoot}nuget/Xamarin.AndroidX.Leanback.1*-preview.*.nupkg",
            $"{externalRoot}nuget/Xamarin.AndroidX.Lifecycle.Common.2*-preview.*.nupkg"
        };
        var androidNugets = new [] {
            $"{externalRoot}legacy/nuget/Xamarin.Android.Arch.Lifecycle.Common.*.nupkg",
            $"{externalRoot}legacy/nuget/Xamarin.Android.Support.v7.AppCompat.*.nupkg",
            $"{externalRoot}legacy/nuget/Xamarin.Android.Support.v17.Leanback.*.nupkg"
        };

        EnsureDirectoryExists($"{testAssembliesFolder}AndroidX");
        foreach(var nuget in androidXNugets.SelectMany(n => GetFiles(n)))
        {
            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            CreateDirectory(tmp);
            Unzip(nuget, tmp);
            CopyFiles(GetFiles($"{tmp}/lib/monoandroid90/*.dll"),  $"{testAssembliesFolder}AndroidX");
            DeleteDirectory(tmp, new DeleteDirectorySettings { Recursive = true, Force = true });
        }

        EnsureDirectoryExists($"{testAssembliesFolder}Android");
        foreach(var nuget in androidNugets.SelectMany(n => GetFiles(n)))
        {
            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            CreateDirectory(tmp);
            Unzip(nuget, tmp);
            CopyFiles(GetFiles($"{tmp}/lib/monoandroid90/*.dll"),  $"{testAssembliesFolder}Android");
            DeleteDirectory(tmp, new DeleteDirectorySettings { Recursive = true, Force = true });
        }
    }
});

Task("NuGets")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    DeleteFiles("./output/nugets/*.nupkg");

    // pack any .nuspec files
    foreach (var ns in GetFiles("./nugets/*.nuspec")) {
        var nuspec = ns;
        var tempNuspec = ns + ".mac.nuspec";
        if (!IsRunningOnWindows()) {
            CopyFile(ns, tempNuspec);
            ReplaceTextInFiles(tempNuspec, "\\", "/");
            nuspec = tempNuspec;
        }

        NuGetPack(nuspec, new NuGetPackSettings {
            ToolPath = NUGET_EXE,
            OutputDirectory = "./output/nugets/",
            RequireLicenseAcceptance = true,
            Version = BUILD_PACKAGE_VERSION,
        });

        if (FileExists(tempNuspec))
            DeleteFile(tempNuspec);
    }

    // pack the tool
    var tool = "./source/Xamarin.AndroidX.Migration/Tool/Xamarin.AndroidX.Migration.Tool.csproj";
    DotNetCorePack(tool, new DotNetCorePackSettings {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "./output/nugets/",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("PackageVersion", BUILD_PACKAGE_VERSION),
    });

    // pack the 
    var migrator = "./source/VisualStudio.AndroidX.Migration/Core/Core.csproj";
    var settings = new MSBuildSettings {
        Configuration = configuration,
        Properties = {
            { "PackageOutputPath", new [] { MakeAbsolute((DirectoryPath)"./output/nugets/").FullPath } },
        },
        BinaryLogger = new MSBuildBinaryLogSettings
        {
            Enabled = true,
            FileName = MakeAbsolute((DirectoryPath)"./output/pack.binlog").FullPath,
        }
    };
    MSBuild(migrator, settings.WithTarget("Pack"));
});

Task("Samples")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    // delete old nugets
    if (DirectoryExists("./externals/packages/Xamarin.AndroidX.Migration"))
        DeleteDirectory("./externals/packages/Xamarin.AndroidX.Migration", true);

    // build the samples
    var sampleProjects = GetFiles("./samples/*/*.sln");
    foreach (var proj in sampleProjects) {
        MSBuild(proj, new MSBuildSettings {
            Configuration = configuration,
            Restore = true,
            MaxCpuCount = 0,
            Properties = {
                { "DesignTimeBuild", new [] { "false" } },
                { "AndroidSdkBuildToolsVersion", new [] { "28.0.3" } },
                { "JavaSdkDirectory", new [] { JAVA_HOME } },
            },
        });
    }
});

Task("Default")
    .IsDependentOn("NativeAssets")
    .IsDependentOn("Libraries")
    .IsDependentOn("NuGets")
    .IsDependentOn("Tests")
    // .IsDependentOn("Samples")
    ;

RunTarget(target);
