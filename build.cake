#tool "nuget:https://www.nuget.org/api/v2?package=xunit.runner.console&version=2.4.1"

var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta02";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";

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

Task("NativeAssets")
    .IsDependentOn("JavaProjects")
    .IsDependentOn("JetifierWrapper");

Task("Libraries")
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
    var testProjects = GetFiles("./tests/*.Tests/*.csproj");
    foreach (var proj in testProjects) {
        DotNetCoreTest(proj.FullPath, new DotNetCoreTestSettings {
            Configuration = configuration,
            NoBuild = true,
        });
    }
});

Task("Default")
    .IsDependentOn("NativeAssets")
    .IsDependentOn("Libraries")
    .IsDependentOn("Tests");

RunTarget(target);
