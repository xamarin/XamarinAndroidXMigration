using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Cecilfier.MSBuildTask
{
    public class CecilizeTask : Microsoft.Build.Utilities.Task
    {
        [Microsoft.Build.Framework.Required]
        public string IntermediateOutputPath
        {
            get;
            set;
        }

        public override bool Execute()
        {
            Log.LogMessage($"CecilizeTask mc++");
            Log.LogMessage($"   IntermediateOutputPath          = {IntermediateOutputPath}");


            string directory = IntermediateOutputPath;
            string file = "";

            List<string> dlls = new List<string>(
                                                    Directory.EnumerateFiles
                                                                (
                                                                    directory,
                                                                    file,
                                                                    SearchOption.AllDirectories
                                                                )
                                                )
                                                .Where(x => ! x.Contains("linksrc"))
                                                .Where(x => ! x.Contains("android/assets"))
                                                .Where(x => ! x.Contains(".app/"))
                                                .Where(x => ! x.Contains(".resources.dll"))
                                                .ToList();
                                                ;

            foreach(string dll in dlls)
            {
                Log.LogMessage($"   dll          = {dll}");
            }

            // enforcing proper correlation between Log errors and build results (success and/or failures)
            return !Log.HasLoggedErrors;
        }
    }
}
