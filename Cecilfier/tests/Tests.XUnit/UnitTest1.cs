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
        public void Test1()
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
                if(dll.Contains("linksrc"))
                {
                    continue;
                }
                migrator = new AndroidXMigrator(dll, dll.Replace(".dll", ".migrated.dll"));
                migrator.Migrate();
            }
        }
    }
}
