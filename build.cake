#tool "nuget:?package=xunit.runner.console&version=2.4.1"

#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"

var target = Argument("t", Argument("target", "ci"));
var verbosity = Argument("v", Argument("verbosity", Verbosity.Normal));
var configuration = Argument("c", Argument("configuration", "Release"));

var mappingVersion = "f48a9d16427ddc449e57fe73017cc2fecf52eb3d";
var mappingAssemblies = $"https://raw.githubusercontent.com/xamarin/AndroidX/{mappingVersion}/mappings/androidx-assemblies.csv";
var mappingMappings = $"https://raw.githubusercontent.com/xamarin/AndroidX/{mappingVersion}/mappings/androidx-mapping.csv";

Task("externals")
    .Does(() =>
{
    EnsureDirectoryExists("./externals/mappings/");

    if (!FileExists("./externals/mappings/androidx-assemblies.csv"))
        DownloadFile(mappingAssemblies, "./externals/mappings/androidx-assemblies.csv");
    if (!FileExists("./externals/mappings/androidx-mapping.csv"))
        DownloadFile(mappingMappings, "./externals/mappings/androidx-mapping.csv");
});

Task("tests-externals")
    .Does(() =>
{
    var testAssembliesFolder = "./externals/test-assets/Assemblies/";

    if (DirectoryExists($"{testAssembliesFolder}AndroidX"))
        return;

    var androidXNugets = new [] {
        "https://globalcdn.nuget.org/packages/xamarin.androidx.appcompat.1.1.0.1.nupkg",
        "https://globalcdn.nuget.org/packages/xamarin.androidx.leanback.1.0.0.1.nupkg",
        "https://globalcdn.nuget.org/packages/xamarin.androidx.lifecycle.common.2.2.0.1.nupkg",
    };
    var androidNugets = new [] {
        "https://globalcdn.nuget.org/packages/xamarin.android.arch.lifecycle.common.1.1.1.3.nupkg",
        "https://globalcdn.nuget.org/packages/xamarin.android.support.v7.appcompat.28.0.0.3.nupkg",
        "https://globalcdn.nuget.org/packages/xamarin.android.support.v17.leanback.28.0.0.3.nupkg",
    };

    EnsureDirectoryExists($"{testAssembliesFolder}AndroidX");
    foreach (var nuget in androidXNugets) {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        CreateDirectory(tmp);
        DownloadFile(nuget, $"{tmp}/nuget.nupkg");
        Unzip($"{tmp}/nuget.nupkg", tmp);
        CopyFiles(GetFiles($"{tmp}/lib/monoandroid90/*.dll"), $"{testAssembliesFolder}AndroidX");
        DeleteDirectory(tmp, new DeleteDirectorySettings { Recursive = true, Force = true });
    }

    EnsureDirectoryExists($"{testAssembliesFolder}Android");
    foreach (var nuget in androidNugets) {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        CreateDirectory(tmp);
        DownloadFile(nuget, $"{tmp}/nuget.nupkg");
        Unzip($"{tmp}/nuget.nupkg", tmp);
        CopyFiles(GetFiles($"{tmp}/lib/monoandroid90/*.dll"), $"{testAssembliesFolder}Android");
        DeleteDirectory(tmp, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
});

Task("libs")
    .IsDependentOn("externals")
    .Does(() =>
{
    MSBuild("./VisualStudio.AndroidX.Migration.sln", new MSBuildSettings {
        Configuration = configuration,
        Restore = true,
        BinaryLogger = new MSBuildBinaryLogSettings
        {
            Enabled = true,
            FileName = MakeAbsolute((DirectoryPath)"./output/binlogs/libs.binlog").FullPath,
        }
    });
});

Task("nuget")
    .IsDependentOn("libs")
    .Does(() =>
{
    DeleteFiles("./output/nugets/*.nupkg");

    var csproj = "./VisualStudio.AndroidX.Migration.Core/VisualStudio.AndroidX.Migration.Core.csproj";
    var settings = new MSBuildSettings {
        Configuration = configuration,
        Properties = {
            { "PackageOutputPath", new [] { MakeAbsolute((DirectoryPath)"./output/nugets/").FullPath } },
        },
        BinaryLogger = new MSBuildBinaryLogSettings
        {
            Enabled = true,
            FileName = MakeAbsolute((DirectoryPath)"./output/binlogs/pack.binlog").FullPath,
        }
    };
    MSBuild(csproj, settings.WithTarget("Pack"));
});

Task("tests")
    .IsDependentOn("tests-externals")
    .IsDependentOn("libs")
    .Does(() =>
{
    var testAssemblies = GetFiles($"./VisualStudio.AndroidX.Migration.Test/bin/{configuration}/*/*.Tests.dll");
    foreach (var assembly in testAssemblies) {
        XUnit2(new [] { assembly }, new XUnit2Settings {
            XmlReport = true,
            NoAppDomain = true,
            OutputDirectory = $"./output/test-results/VisualStudio.AndroidX.Migration.Tests",
            WorkingDirectory = assembly.GetDirectory(),
        });
    }
});

Task("ci")
    .IsDependentOn("externals")
    .IsDependentOn("libs")
    .IsDependentOn("nuget")
    .IsDependentOn("tests");

RunTarget(target);
