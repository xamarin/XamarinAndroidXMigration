#tool "nuget:https://www.nuget.org/api/v2?package=xunit.runner.console&version=2.4.1"

var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta02";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";

var azureBuildNumber = "670";
var azureBuildUrl = $"https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds/{azureBuildNumber}/artifacts?artifactName=nuget&%24format=zip&api-version=5.0";

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
    }
});

Task("NativeAssets")
    .IsDependentOn("JavaProjects")
    .IsDependentOn("JetifierWrapper")
    .IsDependentOn("DownloadNativeFacebookSdk")
    .IsDependentOn("DownloadAndroidXAssets");

Task("Libraries")
    .IsDependentOn("DownloadAndroidXAssets")
    .IsDependentOn("NativeAssets")
    .Does(() =>
{
    MSBuild("Xamarin.AndroidX.Migration.sln", new MSBuildSettings {
        Configuration = configuration,
        Restore = true,
        MaxCpuCount = 0,
        Properties = {
            { "DesignTimeBuild", new [] { "false" } },
            { "AndroidSdkBuildToolsVersion", new [] { "28.0.3" } },
        },
    });
});

Task("Tests")
    .IsDependentOn("Libraries")
    .Does(() =>
{
    var testTasks = GetFiles("tests/*.BuildTasks.Tests/*.BuildTasks.Tests.proj");
    foreach (var proj in testTasks) {
        MSBuild(proj);
    }

    var testProjects = GetFiles("./tests/*.Tests/*.csproj");
    foreach (var proj in testProjects) {
        DotNetCoreTest(proj.GetFilename().ToString(), new DotNetCoreTestSettings {
            Configuration = configuration,
            NoBuild = true,
            TestAdapterPath = ".",
            Logger = "xunit",
            WorkingDirectory = proj.GetDirectory(),
        });
    }
});

Task("Default")
    .IsDependentOn("NativeAssets")
    .IsDependentOn("Libraries")
    .IsDependentOn("Tests");

RunTarget(target);
