using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

namespace Tests.XUnit
{
    public class UnitTest1
    {
        [Fact]
        public void Test_Migration_Batch_On_Cecilfier_Assemblies()
        {
            AndroidXMigrator migrator = null;
            List<string> dlls = new List<string>(
                                                    Directory.EnumerateFiles
                                                                (
                                                                    //"../../samples.test.targets",
                                                                    "../../../../../source/Xamarin.AndroidX.Cecilfier/",
                                                                    "*.dll",
                                                                    SearchOption.AllDirectories
                                                                )
                                                )
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

            return;
        }
        //AarxerciseDemoApp
        //NoSupportApp
        
        [Fact]
        public void Test_Migration_BabySteps()
        {
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

            return;
        }
    }
}
