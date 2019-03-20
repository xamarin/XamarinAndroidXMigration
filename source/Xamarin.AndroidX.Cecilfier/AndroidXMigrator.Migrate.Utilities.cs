using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Mono.Cecil;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        Stopwatch timer = null;
        StringBuilder log = null;
        string replacement = null;
        List<string> Problems = new List<string>();

        protected string FindReplacingTypeFromMappings(string typename)
        {
            string r = null;

            if (!typename.Contains("/"))
            {
                // type (not nested type)
            }
            else
            {
                // nested type
                int idx1 = typename.LastIndexOf('.');
                if (idx1 < 0)
                {
                    Problems.Add($"FindReplacingTypeFromMappings type w/o namespace: {typename}");
                    return r;
                }
                string tn = typename.Substring(0, idx1);

                //int idx3 = typename.LastIndexOf('/');
                //string tn_nested = typename.Substring(idx3 + 1, r.Length - idx3 - 1);
                typename = tn;
            }

            int index = ClassMappingsSortedProjected.Span.BinarySearch(typename);
            if (index < 0)
            {
                string msg = "Android.Support class not found in mappings";

                //throw new InvalidOperationException(msg);

                AndroidSupportNotFoundInGoogle.Add(typename);
                Problems.Add($"FindReplacingTypeFromMappings Android.Support class not found in mappings: {typename}");
            }
            else
            {
                r = ClassMappingsSorted.Span[index].AndroidXClassFullyQualified;    
            }
            return r;
        }

        private void MigrateRadeksSample()
        {
            var a = AssemblyDefinition.ReadAssembly ("a.dll", new ReaderParameters { ReadWrite = true, InMemory = true });
            var b = AssemblyDefinition.ReadAssembly ("b.dll", new ReaderParameters { ReadWrite = true, InMemory = true });
            var t = a.MainModule.GetType ("N1.Abc");
            t.Namespace = "N2";
            a.Write ("a.dll");
            b.Write ("b.dll");
        }
    }
}
