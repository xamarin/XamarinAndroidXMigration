///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var TARGET = Argument("t", Argument("target", "Default"));
var CONFIGURATION = Argument("configuration", "Release");

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta02";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";
var jetifierFolderName = "jetifier-standalone";
var jetifierNativePath = "./native/";
var jetifierWrapperFileName = "JetifierWrapper";
var classpathJoinSymbol = IsRunningOnUnix() ? ":" : ";";

Task("Default")
    .IsDependentOn("libs")
    .Does(() =>
{
    
});

Task("libs")
    .IsDependentOn("MSBuild");

Task("MSBuild")
    .IsDependentOn("native")
    .Does(() =>
{
    
});

Task("native")
    .IsDependentOn("externals")
    .Does(() =>
{
    var jarFiles = GetFiles($"{jetifierNativePath}lib/*.jar");
    var classPath = string.Join(classpathJoinSymbol, jarFiles);
    StartProcess("javac", $"-cp {classPath} {jetifierNativePath}src/com/xamarin/androidx/jetifierWrapper/Main.java");

    var processSettings = new ProcessSettings { 
        Arguments = $"cfm ../{jetifierWrapperFileName}.jar ./META-INF/MANIFEST.MF ./com/xamarin/androidx/jetifierWrapper/Main.class ./com/xamarin/androidx/jetifierWrapper/JetifierCallable.class",
        WorkingDirectory = $"{jetifierNativePath}src"
    };
    StartProcess("jar", processSettings);
});

Task("externals")
    .Does(() =>
{
    if (!FileExists ($"{jetifierNativePath}{jetifierFolderName}.zip"))
        DownloadFile(jetifierDownloadUrl, $"{jetifierNativePath}{jetifierFolderName}.zip");
    
    if (!DirectoryExists ($"{jetifierNativePath}{jetifierFolderName}"))
        Unzip($"./native/{jetifierFolderName}.zip", $"./native/");
    
    if (!DirectoryExists ($"{jetifierNativePath}lib") && DirectoryExists ($"{jetifierNativePath}{jetifierFolderName}/lib"))
        MoveDirectory($"./{jetifierFolderName}/lib", $"{jetifierNativePath}lib");
});

RunTarget(TARGET);
