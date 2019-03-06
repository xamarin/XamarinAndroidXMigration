using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

namespace Tests.XUnit
{
    public class UnitTest_App_Programm
    {
        [Fact]
        public void Test_Commandline_Args_API()
        {
            string[] args = new string[]
            {
                "/exact-pairs",
                "     ../../../../../../../../Demo/BabySteps/bin/Debug/BabySteps.dll:../../../../../../../../Demo/BabySteps/bin/Debug/BabySteps.AAAA.dll",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.AAA.dll",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/AarxerciseDemoApp.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/AarxerciseDemoApp.AAA.dll",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.AAA.dll",
                "/bulk-with-pattern",
                "     patern01=migrated-daytime",
                "     dlls01=../../../../*.03.dll",
                "     pattern02:migrated-daytime",
                "     dlls02:../../../../*.04.dll",
                "     pattern03=migrated-daytime",
                "     dlls04=../../../../*.05.dll",
            };
            Xamarin.AndroidX.Cecilfier.App.Program.Main(args);

            return;
        }
        
    }
}
