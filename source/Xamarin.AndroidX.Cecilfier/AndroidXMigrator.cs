using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Core.Linq;

[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("Tests.XUnit")]

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        private static ReadOnlyMemory<char> memory_android_support;
        private static ReadOnlyMemory<char> memory_androidx;

        private static Memory<Memory<char>> TypeNamesToSkip;

        static partial void InitializeAnalysis();

        static AndroidXMigrator()
        {
            AbstractSyntaxTree = new AST.AbstractSyntaxTree();

            Parallel.Invoke
                        (
                            () =>
                            {
                                string file = Path.Combine
                                                    (
                                                        new string[]
                                                        {
                                                            Settings.WorkingDirectory,
                                                            "mappings",
                                                            "MigrationMappings.csv"
                                                        }
                                                    );
                                LoadMappingsClasses(file);
                                InitializePerformance();
                            }
                            //() =>
                            //{
                            //    Initialize();
                            //}
                        );

            InitializeAnalysis();

            return;
        }

        public static HashSet<string> AndroidSupportNotFoundInGoogle = new HashSet<string>();


        public static void LoadMappingsClasses(string file)
        {
            Trace.WriteLine($" reading csv = {file}");
            Core.Text.CharacterSeparatedValues csv = new Core.Text.CharacterSeparatedValues();
            string content = csv.LoadAsync(file).Result;

            Trace.WriteLine($"    parsing csv...");
            IEnumerable<string[]> mapping = csv
                                            .ParseTemporaryImplementation()
                                            .ToList()
                                            ;
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                    mapping_strongly_typed;

            mapping_strongly_typed = Convert_ClassMappings(mapping);

            ClassMappings = mapping_strongly_typed
                                            .ToList().AsReadOnly()
                                            // hangs if used Memory
                                            //.ToArray()
                                            //.AsMemory()
                                            ;

            return;
        }

        private static void InitializePerformance()
        {
            map_sorted_tn_xm_as = ClassMappings
                                        //.Where(i => !string.IsNullOrEmpty(i.TypenameFullyQualifiedAndroidSupport))
                                        .OrderBy(i => i.TypenameFullyQualifiedXamarinAndroidSupport)
                                        .ToArray()
                                        .AsMemory()
                                        ;

            //int n = map_sorted_tn_xm_as.Length;

            // searching after managed Xamarin Android.Support mapping to find AndroidX type
            map_sorted_tn_xm_as_index = map_sorted_tn_xm_as
                                            .Select(i => i.TypenameFullyQualifiedXamarinAndroidSupport)
                                            .ToArray()
                                            .AsMemory()
                                            ;


            map_sorted_tn_java_as = ClassMappings
                                        //.Where(i => !string.IsNullOrEmpty(i.TypenameFullyQualifiedAndroidSupport))
                                        .OrderBy(i => i.TypenameFullyQualifiedAndroidSupport)
                                        .ToArray()
                                        .AsMemory()
                                        ;

            map_sorted_tn_java_as_index = map_sorted_tn_java_as
                                            .Select(i => i.TypenameFullyQualifiedAndroidSupport)
                                            .ToArray()
                                            .AsMemory()
                                            ;

            // TODO: move this to unit tests
            string classname = "Android.Support.CustomTabs.CustomTabsServiceConnection";
            int idx = map_sorted_tn_xm_as_index.Span.BinarySearch(classname);
            //if( idx != 603 )
            //{
            //    string msg =
            //        "Android.Support sorted classnames changed"
            //        + Environment.NewLine +
            //        "Could be change in mappings or bindings!"
            //        + Environment.NewLine +
            //        "CHECK!!!!"
            //        ;

            //    throw new InvalidDataException(msg);
            //}

            return;
        }

        //-------------------------------------------------------------------------------------------------------------------
        public static
            ReadOnlyCollection
            //ReadOnlyMemory
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                ClassMappings
        {
            get;
            private set;
        }

        public static 
            IEnumerable
                   <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX,
                        string TypenameFullyQualifiedXamarinAndroidSupport,
                        string TypenameFullyQualifiedXamarinAndroidX
                    )
                >
                Convert_ClassMappings(IEnumerable<string[]> untyped_data)
        {
             Trace.WriteLine($"    converting csv to strongly typed data");
           //int n = untyped_data.Count();

            //for(int i = 0; i < n; i++)
            foreach(string[] row in untyped_data)
            {
                //string[] row = untyped_data[i];

                //TODO - benchmarking
                //Span<string> row_data = row.AsSpan();

                yield return
                        (
                            TypenameFullyQualifiedAndroidSupport: row[0],
                            TypenameFullyQualifiedAndroidX: row[1],
                            TypenameFullyQualifiedXamarinAndroidSupport: row[2],
                            TypenameFullyQualifiedXamarinAndroidX: row[3]
                        );
            }
        }
        //-------------------------------------------------------------------------------------------------------------------

        private static void Initialize()
        {
            Trace.WriteLine($"    Initialize...");
            memory_android_support = string.Intern("Android.Support").AsMemory();
            memory_androidx = string.Intern("AndroidX").AsMemory();

            TypeNamesToSkip = 
                            new Memory<char>[]
                                    {
                                        string.Intern("<Module>").ToCharArray().AsMemory(),
                                        string.Intern("__TypeRegistrations").ToCharArray().AsMemory(),
                                    }
                                    .AsMemory();

            PerformanceData = new List
                                    <
                                        (
                                            string Assembly,
                                            string Algorithm, // Matthew's Shortcuts, Redth's Original Patch
                                            long Duration
                                        )
                                    >();

            return;
        }

        public AndroidXMigrator(string input, string output)
        {
            this.PathAssemblyInput = input;
            this.PathAssemblyOutput = output;

            Initialize();
            //InitializePerformance();

            return;
        }

        public string PathAssemblyInput
        {
            get;
            set;
        }

        public string PathAssemblyOutput
        {
            get;
            set;
        }

        partial void MigrateWithSpanMemory(ref long duration);

        public void Migrate(bool span_memory_implementation = false)
        {
            long duration = -1;

            if (span_memory_implementation)
            {
                MigrateWithSpanMemory(ref duration); 
            }
            else
            {
                MigrateWithWithStringsOriginalPatchByRedth(ref duration);
                PerformanceData.Add((PathAssemblyOutput, "Redth Original", duration));
                //MigrateWithWithStringsMathewsShortcuts(ref duration);
                //PerformanceData.Add((PathAssemblyOutput, "Matthews Shortcuts", duration));
            }

            string fp = Path.ChangeExtension(this.PathAssemblyInput, "problems.txt");
            string fasm = Path.ChangeExtension(this.PathAssemblyInput, "android-support-missing.txt");
            File.WriteAllLines(fp, this.Problems.ToList());
            File.WriteAllLines(fp, AndroidSupportNotFoundInGoogle.ToList());

            return;
        }


        static string row =
                    $@"
	    [InlineData
            (
			    ""JNI_AS"",
			    ""JNI_AX""
            )
        ]
                        ";
        static string jni_as = null;
        static string jni_ax = null;

        public void Dump()
        {
            List<string> dump = new List<string>();

            Parallel.Invoke
                (
                    () =>
                    {
                        foreach (var migrated_jni in MigratedJNI)
                        {
                            string r = row;
                            r = r.Replace("JNI_AS", migrated_jni.AndroidSupportJNI);
                            r = r.Replace("JNI_AX", migrated_jni.AndroidXJNI);

                            dump.Add(r);
                        }

                        string text = string.Join(Environment.NewLine, dump);
                            File.WriteAllText(Path.ChangeExtension($"{PathAssemblyInput}", "MigratedJNI.csv"), text);
                    }
                );

        }

    }
}
