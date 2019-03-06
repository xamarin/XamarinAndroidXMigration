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
        public void Test_Commandline_Args_API_Matthews_Demo()
        {
            string[] args = new string[]
            {
                "/exact-pairs",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.AAA.dll",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/AarxerciseDemoApp.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/AarxerciseDemoApp.AAA.dll",
                "     ../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.dll:../../../../../../../../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.AAA.dll",
                "     ../../../../../../../../Demo/BabySteps/bin/Debug/BabySteps.dll:../../../../../../../../Demo/BabySteps/bin/Debug/BabySteps.AAAA.dll",
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
        
        [Fact]
        public void Test_Commandline_Args_API_AndroidSupportMerged()
        {
            string[] args = new string[]
            {
                "/exact-pairs",
                "     ../../../../../../../tests/artifacts/AndroidSupport.Merged.dll:../../../../../../../tests/artifacts/AndroidSupport.Merged.AAA.dll",
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
