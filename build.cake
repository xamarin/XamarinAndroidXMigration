///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("t", Argument("target", "Default"));
var configuration = Argument("configuration", "Release");

Task("NativeLibrary")
    .Does(() =>
{
    var nativeProjects = new [] {
        "tests/Aarxersise.Java.AndroidX",
        "tests/Aarxersise.Java.Support"
    };

    foreach (var native in nativeProjects) {
        if (IsRunningOnWindows()) {
            StartProcess($"{native}/gradlew.bat", $"assembleDebug -p {native}");
        } else {
            StartProcess("bash", $"{native}/gradlew assembleDebug -p {native}");
        }
    }
});

Task("Default")
    .IsDependentOn("NativeLibrary")
    .Does(() =>
{
    MSBuild("Demo/Aarxercise.sln");
});

Task("Jetifier")
    .IsDependentOn("JetifierMSBuild");

Task("JetifierMSBuild")
    .IsDependentOn("JetifierNative")
    .Does(() =>
{
});

Task("JetifierNative")
    .Does(() =>
{
    Warning ("Test! JetifierNative");
    CakeExecuteScript ("./Jetifier/build.cake");
});

RunTarget(target);
