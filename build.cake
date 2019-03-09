#tool "nuget:https://www.nuget.org/api/v2?package=xunit.runner.console&version=2.4.1"

var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

Task("NativeAssets")
    .Does(() =>
{
    var settings = new CakeSettings {
        Arguments = {
            { "verbosity", verbosity },
            { "configuration", configuration },
        }
    };

    // build the Jetifier wrapper
    CakeExecuteScript("./Jetifier/build.cake", settings);

    // build the assets for the tests
    CakeExecuteScript("./tests/build.cake", settings);
});

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
