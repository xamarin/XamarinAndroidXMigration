#tool "nuget:?package=xunit.runner.console&version=2.4.1"

#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"

var target = Argument("t", Argument("target", "ci"));
var verbosity = Argument("v", Argument("verbosity", Verbosity.Normal));
var configuration = Argument("c", Argument("configuration", "Release"));

var SUPPORT_AZURE_BUILD_ID = EnvironmentVariable("SUPPORT_AZURE_BUILD_ID") ?? "10305";
var ANDROIDX_AZURE_BUILD_ID = EnvironmentVariable("ANDROIDX_AZURE_BUILD_ID") ?? "10297";

var BUILD_VERSION = EnvironmentVariable("BUILD_VERSION") ?? "1.0.0";
var BUILD_PACKAGE_VERSION = EnvironmentVariable("BUILD_PACKAGE_VERSION") ?? "1.0.0-preview";

var legacyBuildUrl = $"https://dev.azure.com/xamarin/public/_apis/build/builds/{SUPPORT_AZURE_BUILD_ID}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";
var azureBuildUrl = $"https://dev.azure.com/xamarin/public/_apis/build/builds/{ANDROIDX_AZURE_BUILD_ID}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";

void RunGradle(DirectoryPath root, string target)
{
    root = MakeAbsolute(root);
    var proc = IsRunningOnWindows()
        ? root.CombineWithFilePath("gradlew.bat").FullPath
        : "bash";
    var args = IsRunningOnWindows()
        ? ""
        : root.CombineWithFilePath("gradlew").FullPath;
    args += $" {target} -p {root}";

    var exitCode = StartProcess(proc, args);
    if (exitCode != 0)
        throw new Exception($"Gradle exited with code {exitCode}.");
}

Task("JetifierWrapper")
    .Does(() =>
{
    var root = "./source/Xamarin.AndroidX.Migration/jetifierWrapper/";

    RunGradle(root, "jar");

    var outputDir = MakeAbsolute((DirectoryPath)"./output/JetifierWrapper");
    EnsureDirectoryExists(outputDir);

    CopyFileToDirectory($"{root}build/libs/jetifierWrapper-1.0.jar", outputDir);
    Zip(outputDir, "./output/JetifierWrapper.zip");
});

Task("JavaProjects")
    .Does(() =>
{
    var nativeProjects = new [] {
        "./tests/Xamarin.AndroidX.Migration/Aarxersise.Java.AndroidX/",
        "./tests/Xamarin.AndroidX.Migration/Aarxersise.Java.Support/",
        "./samples/com.xamarin.CoolLibrary/",
    };

    foreach (var native in nativeProjects) {
        RunGradle (native, "assembleDebug");
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
        CopyFileToDirectory($"{externalsRoot}nuget/dependencies.json", "mappings");
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
    var failed = false;

    // test projects using dotnet core
    var testProjects = GetFiles("./tests/Xamarin.AndroidX.Migration/**/*.Tests.csproj");
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
            failed = true;
            Error("Tests failed: " + ex.Message);
            Error(ex);
        }
    }

    // test projects using xunit test runner
    var testAssemblies = GetFiles("./tests/VisualStudio.AndroidX.Migration/*/bin/Release/*/*.Tests.dll");
    foreach (var assembly in testAssemblies) {
        try {
            XUnit2(new [] { assembly }, new XUnit2Settings {
                XmlReport = true,
                NoAppDomain = true,
                OutputDirectory = $"./output/test-results/VisualStudio.AndroidX.Migration.Tests",
                WorkingDirectory = assembly.GetDirectory(),
            });
        } catch (Exception ex) {
            failed = true;
            Error("Tests failed: " + ex.Message);
            Error(ex);
        }
    }

    if (failed)
        throw new Exception("Some tests failed.");
});

Task("VSTestPrepare")
    .IsDependentOn("DownloadAndroidXAssets")
    .IsDependentOn("DownloadLegacyAssets")
    .Does(() =>
{
    var externalRoot = "./externals/";
    var testAssembliesFolder = "./externals/test-assets/vs-tests/Assemblies/";

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
            },
        });
    }
});

Task("ci")
    .IsDependentOn("NativeAssets")
    .IsDependentOn("Libraries")
    .IsDependentOn("NuGets")
    .IsDependentOn("Tests")
    // .IsDependentOn("Samples")
    ;

RunTarget(target);
