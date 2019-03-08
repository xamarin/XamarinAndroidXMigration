///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var TARGET = Argument ("t", Argument ("target", "Default"));
var CONFIGURATION = Argument ("configuration", "Release");

var jetifierVersion = "1.0.0";
var jetifierBetaVersion = "-beta02";
var jetifierDownloadUrl = $"https://dl.google.com/dl/android/studio/jetifier-zips/{jetifierVersion}{jetifierBetaVersion}/jetifier-standalone.zip";
var jetifierFolderName = "jetifier-standalone";
var jetifierNativePath = "./native/";
var jetifierWrapperFileName = "JetifierWrapper";
var classpathJoinSymbol = IsRunningOnUnix () ? ":" : ";";

Task ("Default")
	.IsDependentOn ("libs")
	.Does (() => {
	
});

Task ("libs")
	.IsDependentOn ("MSBuild");

Task ("msbuild")
	.IsDependentOn ("native")
	.Does (() => {
	MSBuild("./MSBuild/JetifierWrapper.sln", new MSBuildSettings { Configuration = CONFIGURATION });
});

Task ("msbuild-test")
	.IsDependentOn ("msbuild")
	.Does (() => {
	var testPath = "./test/";
	var msBuildPath = "./MSBuild/";
	var facebookFilename = "facebook-android-sdk";
	var facebookVersion = "4.40.0";
	var facebookFullName = $"{facebookFilename}-{facebookVersion}";
	var facebookTestUrl = $"https://origincache.facebook.com/developers/resources/?id={facebookFullName}.zip";

	EnsureDirectoryExists (testPath);

	if (!FileExists ($"{testPath}{facebookFullName}.zip")) {
		DownloadFile (facebookTestUrl, $"{testPath}{facebookFullName}.zip");
		Unzip ($"{testPath}{facebookFullName}.zip", testPath);
	}

	if (!FileExists ($"{testPath}{jetifierWrapperFileName}.zip")) {
		CopyFile ($"{jetifierNativePath}{jetifierWrapperFileName}.zip", $"{testPath}{jetifierWrapperFileName}.zip");
		Unzip ($"{testPath}{jetifierWrapperFileName}.zip", testPath);
	}

	CopyFile ($"{msBuildPath}{jetifierWrapperFileName}/bin/Release/{jetifierWrapperFileName}.dll", $"{testPath}{jetifierWrapperFileName}.dll");

	MSBuild($"{testPath}{jetifierWrapperFileName}Test.csproj");
});

Task ("native")
	.IsDependentOn ("externals")
	.Does (() => {
	var jarFiles = GetFiles ($"{jetifierNativePath}lib/*.jar");
	var classPath = string.Join (classpathJoinSymbol, jarFiles);
	StartProcess ("javac", $"-cp {classPath} {jetifierNativePath}src/com/xamarin/androidx/jetifierWrapper/Main.java");

	var processSettings = new ProcessSettings { 
		Arguments = $"cfm ../{jetifierWrapperFileName}.jar ./META-INF/MANIFEST.MF ./com/xamarin/androidx/jetifierWrapper/Main.class ./com/xamarin/androidx/jetifierWrapper/JetifierCallable.class",
		WorkingDirectory = $"{jetifierNativePath}src"
	};
	StartProcess ("jar", processSettings);

	var jetifierFiles = new FilePathCollection (GetFiles ($"{jetifierNativePath}lib/*.jar"));
	jetifierFiles.Add (new FilePath ($"{jetifierNativePath}{jetifierWrapperFileName}.jar"));

	Zip (jetifierNativePath, $"{jetifierNativePath}{jetifierWrapperFileName}.zip", jetifierFiles);
});

Task ("native-test")
	.IsDependentOn ("native")
	.Does (() => {
	var testPath = "./test/";
	var facebookFilename = "facebook-android-sdk";
	var facebookVersion = "4.40.0";
	var facebookFullName = $"{facebookFilename}-{facebookVersion}";
	var facebookTestUrl = $"https://origincache.facebook.com/developers/resources/?id={facebookFullName}.zip";

	EnsureDirectoryExists (testPath);

	if (!FileExists ($"{testPath}{facebookFullName}.zip")) {
		DownloadFile (facebookTestUrl, $"{testPath}{facebookFullName}.zip");
		Unzip ($"{testPath}{facebookFullName}.zip", testPath);
	}

	if (!FileExists ($"{testPath}{jetifierWrapperFileName}.zip")) {
		CopyFile ($"{jetifierNativePath}{jetifierWrapperFileName}.zip", $"{testPath}{jetifierWrapperFileName}.zip");
		Unzip ($"{testPath}{jetifierWrapperFileName}.zip", testPath);
	}

	var javaMainClass = "com.xamarin.androidx.jetifierWrapper.Main";
	var jetifierJarFile = new FilePath ($"{testPath}JetifierWrapper.jar");

	Func<IFileSystemInfo, bool> excludeJetifiedAars =
	 fileSystemInfo => !fileSystemInfo.Path.FullPath.EndsWith(
		 "jetified.aar", StringComparison.OrdinalIgnoreCase);

	var aarFiles = GetFiles ($"{testPath}{facebookFullName}/**/*.aar", new GlobberSettings { Predicate = excludeJetifiedAars });
	var jarFiles = GetFiles ($"{testPath}lib/*.jar");

	var inputFiles = new List<string> ();
	var outputFiles = new List<string> ();

	foreach (var aarFile in aarFiles) {
		inputFiles.Add (aarFile.FullPath);
		outputFiles.Add ($"{aarFile.GetDirectory ()}/{aarFile.GetFilenameWithoutExtension ()}-jetified{aarFile.GetExtension ()}");
	}

	var classPath = string.Join (classpathJoinSymbol, jarFiles);
	var inputs = $"-i {string.Join (" -i ", inputFiles)}";
	var outputs = $"-o {string.Join (" -o ", outputFiles)}";

	StartProcess ("java", $"-classpath \"{jetifierJarFile}{classpathJoinSymbol}{classPath}\" {javaMainClass} {inputs} {outputs}");
});

Task ("externals")
	.Does (() => {
	if  (!FileExists ($"{jetifierNativePath}{jetifierFolderName}.zip"))
		DownloadFile (jetifierDownloadUrl, $"{jetifierNativePath}{jetifierFolderName}.zip");
	
	if (!DirectoryExists ($"{jetifierNativePath}{jetifierFolderName}"))
		Unzip ($"{jetifierNativePath}{jetifierFolderName}.zip", jetifierNativePath);
	
	if (!DirectoryExists ($"{jetifierNativePath}lib") && DirectoryExists ($"{jetifierNativePath}{jetifierFolderName}/lib"))
		MoveDirectory ($"{jetifierNativePath}{jetifierFolderName}/lib", $"{jetifierNativePath}lib");
});

RunTarget (TARGET);
