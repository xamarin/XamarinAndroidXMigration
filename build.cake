///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("NativeLibrary")
    .Does(() =>
{
    var ext = IsRunningOnWindows() ? ".bat" : "";
    StartProcess("Aar/Aarxercise/gradlew" + ext, "assembleDebug -p Aar/Aarxercise");
});

Task("Default")
    .IsDependentOn("NativeLibrary")
    .Does(() =>
{
    MSBuild("Demo/Aarxercise.sln");
});

RunTarget(target);