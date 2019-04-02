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

        //-------------------------------------------------------------------------------------------
        // Android Support for searching MANAGED types
        // sorted for BinarySearch
        private static Memory
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX,
                        string TypenameFullyQualifiedXamarinAndroidSupport,
                        string TypenameFullyQualifiedXamarinAndroidX
                    )
                > map_sorted_tn_xm_as;

        // "index" for searching
        private static Memory<string> map_sorted_tn_xm_as_index;
        //-------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------
        // Android Support for searching JAVA types
        // sorted for BinarySearch
        private static Memory
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX,
                        string TypenameFullyQualifiedXamarinAndroidSupport,
                        string TypenameFullyQualifiedXamarinAndroidX
                    )
                > map_sorted_tn_java_as;

        // "index" for searching
        private static Memory<string> map_sorted_tn_java_as_index;
        //-------------------------------------------------------------------------------------------

        protected string FindReplacingTypeFromMappingsManaged(string typename)
        {
            string r = null;

            if ( ! typename.StartsWith("Android.Support", StringComparison.Ordinal))
            {
                return r;
            }

            if (!typename.Contains("/"))
            {
                // type (nested type)
            }
            else
            {
                // nested type
                int idx1 = typename.LastIndexOf('.');
                if (idx1 < 0)
                {
                    Problems.Add($"FindReplacingTypeFromMappingsManaged type w/o namespace: {typename}");
                    return r;
                }
                string tn = typename.Substring(0, idx1);

                //int idx3 = typename.LastIndexOf('/');
                //string tn_nested = typename.Substring(idx3 + 1, r.Length - idx3 - 1);
                typename = tn;
            }

            int index = map_sorted_tn_xm_as_index.Span.BinarySearch(typename);
            if (index < 0)
            {
                string msg = "Android.Support MANAGED class not found in mappings";

                //throw new InvalidOperationException(msg);

                AndroidSupportNotFoundInGoogle.Add(typename);
                Problems.Add($"FindReplacingTypeFromMappingsManaged Android.Support class not found in mappings: {typename}");
            }
            else
            {
                r = map_sorted_tn_xm_as.Span[index].TypenameFullyQualifiedXamarinAndroidX;

                Trace.WriteLine($"Mapping found");
                Trace.WriteLine($"   typename   = {typename}");
                Trace.WriteLine($"   to");
                Trace.WriteLine($"   r          = {r}");
            }

            return r;
        }

        protected string FindReplacingTypeFromMappingsJava(string typename)
        {
            string r = null;

            int index = map_sorted_tn_java_as_index.Span.BinarySearch(typename);
            if (index < 0)
            {
                string msg = "Android.Support java class not found in mappings";

                //throw new InvalidOperationException(msg);

                AndroidSupportNotFoundInGoogle.Add(typename);
                Problems.Add($"FindReplacingTypeFromMappingsJava Android.Support class not found in mappings: {typename}");
            }
            else
            {
                r = map_sorted_tn_xm_as.Span[index].TypenameFullyQualifiedAndroidX;

                Trace.WriteLine($"Mapping found");
                Trace.WriteLine($"   typename   = {typename}");
                Trace.WriteLine($"   to");
                Trace.WriteLine($"   r          = {r}");
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
