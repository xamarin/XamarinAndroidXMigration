#! "netcoreapp2.2"
#r "nuget: System.Memory 4.5.2"
#r "../source/Xamarin.AndroidX.Cecilfier/bin/Debug/netstandard2.0/Xamarin.AndroidX.Cecilfier.dll"

/*
dotnet script ./Cecilfier.dotnet-script.csx
dotnet script --debug ./Cecilfier.dotnet-script.csx
*/
using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

AndroidXMigrator migrator = null;
List<string> dlls = new List<string>(
                                        Directory.EnumerateFiles
                                                    (
                                                        //"../../samples.test.targets",
                                                        "../../../../../../Demo/BabySteps/bin/Debug/",
                                                        "BabySteps.dll",
                                                        SearchOption.TopDirectoryOnly
                                                    )
                                    )
                                    .Where(x => ! x.Contains("linkdst"))
                                    .Where(x => ! x.Contains("linksrc"))
                                    .Where(x => ! x.Contains("android/assets"))
                                    .Where(x => ! x.Contains(".app/"))
                                    .Where(x => ! x.Contains(".resources.dll"))
                                    .ToList();
                                    ;
        
foreach (string dll in dlls)
{
    string msg = $"androidx-migrated-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}";
    migrator = new AndroidXMigrator(dll, dll.Replace(".dll", $".{msg}.dll"));
    migrator.Migrate();
}
