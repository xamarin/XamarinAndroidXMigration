///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("t", Argument("target", "Default"));
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
