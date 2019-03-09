var target = Argument("t", Argument("target", "Default"));
var verbosity = Argument("v", Argument("verbosity", "Normal"));
var configuration = Argument("c", Argument("configuration", "Release"));

Task("JavaProjects")
    .Does(() =>
{
    var nativeProjects = new [] {
        "Aarxersise.Java.AndroidX",
        "Aarxersise.Java.Support",
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

Task("Default")
    .IsDependentOn("JavaProjects");

RunTarget(target);
