#tool "nuget:https://www.nuget.org/api/v2?package=xunit.runner.console&version=2.4.1"
#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"

var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

var nugetPath = Context.Tools.Resolve("nuget.exe");

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta05";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";

var azureBuildNumber = "3842";
var azureBuildUrl = $"https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds/{azureBuildNumber}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";

var PACKAGE_VERSION = EnvironmentVariable("PACKAGE_VERSION") ?? "1.0.0";
var PREVIEW_LABEL = EnvironmentVariable("PREVIEW_LABEL") ?? "preview";
var BUILD_NUMBER = EnvironmentVariable("BUILD_NUMBER") ?? "";
if (string.IsNullOrEmpty(BUILD_NUMBER)) {
    BUILD_NUMBER = "0";
}

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
    var jetifierWrapperRoot = MakeAbsolute((DirectoryPath)"./source/com.xamarin.androidx.jetifierWrapper");
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
        "tests/Aarxersise.Java.AndroidX",
        "tests/Aarxersise.Java.Support",
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

Task("DownloadXamarinFacebookSdk")
    .Does(() =>
{
    var sdkRoot = "./externals/test-assets/xamarin-facebook-sdk/";

    string [] facebookSdks = { "AppLinks", "Common", "Core", "Login", "Marketing", "Places", "Share" };
    var facebookFilename = "Xamarin.Facebook.{0}.Android";
    var facebookVersion = "4.40.0";
    var facebookNugets = facebookSdks.Select(sdk => string.Format(facebookFilename, sdk));

    EnsureDirectoryExists(sdkRoot);

    NuGetInstall(facebookNugets, new NuGetInstallSettings {
        ToolPath = nugetPath,
        Version = facebookVersion,
        ExcludeVersion = true,
        OutputDirectory = sdkRoot
    });
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

Task("NativeAssets")
    .IsDependentOn("JavaProjects")
    .IsDependentOn("JetifierWrapper")
    .IsDependentOn("DownloadNativeFacebookSdk")
    .IsDependentOn("DownloadXamarinFacebookSdk")
    .IsDependentOn("DownloadAndroidXAssets");

Task("Libraries")
    .IsDependentOn("JetifierWrapper")
    .IsDependentOn("DownloadAndroidXAssets")
    .IsDependentOn("NativeAssets")
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

    // copy the androidx-migrator tools
    foreach (var tf in new [] { "net47", "netcoreapp2.2" }) {
        var root = $"./source/Xamarin.AndroidX.Migration.Tool/bin/{configuration}/{tf}";
        var outRoot = $"./output/androidx-migrator/{tf}";
        EnsureDirectoryExists($"{outRoot}/Tools/");
        CopyDirectory($"{root}/Tools/", $"{outRoot}/Tools/");
        CopyFiles($"{root}/Mono.*", $"{outRoot}/");
        CopyFiles($"{root}/Xamarin.*", $"{outRoot}/");
        Zip($"{outRoot}/", $"./output/androidx-migrator.zip");
    }

    // copy the build tasks
    {
        var root = $"./source/Xamarin.AndroidX.Migration.BuildTasks/bin/{configuration}/net47";
        var outRoot = $"./output/Xamarin.AndroidX.Migration.BuildTasks";
        EnsureDirectoryExists($"{outRoot}/build/Tools/");
        CopyDirectory($"{root}/Tools/", $"{outRoot}/build/Tools/");
        CopyFiles($"{root}/Mono.*", $"{outRoot}/build/");
        CopyFiles($"{root}/Xamarin.*", $"{outRoot}/build/");
        Zip($"{outRoot}/", $"./output/Xamarin.AndroidX.Migration.BuildTasks.zip");
    }
});

Task("Tests")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    var testProjects = GetFiles("./tests/*.Tests/*.csproj");
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

Task("NuGets")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    DeleteFiles("./output/nugets/*.nupkg");

    var stableVersion = $"{PACKAGE_VERSION}";
    var previewVersion = $"{PACKAGE_VERSION}-{PREVIEW_LABEL}.{BUILD_NUMBER}";

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
            Version = stableVersion,
        });
        NuGetPack(nuspec, new NuGetPackSettings {
            OutputDirectory = "./output/nugets/",
            RequireLicenseAcceptance = true,
            Version = previewVersion,
        });

        if (FileExists(tempNuspec)) {
            DeleteFile(tempNuspec);
        }
    }

    var tool = "./source/Xamarin.AndroidX.Migration.Tool/Xamarin.AndroidX.Migration.Tool.csproj";
    DotNetCorePack(tool, new DotNetCorePackSettings {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "./output/nugets/",
        ArgumentCustomization = args => args
            .Append("/p:PackAsTool=True")
            .Append($"/p:PackageVersion={stableVersion}"),
    });
    DotNetCorePack(tool, new DotNetCorePackSettings {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "./output/nugets/",
        ArgumentCustomization = args => args
            .Append("/p:PackAsTool=True")
            .Append($"/p:PackageVersion={previewVersion}"),
    });
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

                // a flag to ensure we use the nugets
                { "UseMigratorNuGetPackages", new [] { "true" } },
                // make sure to restore to a temporary location
                { "RestoreNoCache", new [] { "true" } },
                { "RestorePackagesPath", new [] { "./externals/packages" } },
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
