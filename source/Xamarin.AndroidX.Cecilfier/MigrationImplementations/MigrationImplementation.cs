using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Linq;
using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

namespace Xamarin.AndroidX.Cecilfier.MigrationImplementations
{
    public abstract partial class MigrationImplementation
    {
        public MigrationImplementation(AndroidXMigrator migrator)
        {
            androidx_migrator = migrator;

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
                                                            "API.Mappings.Merged.Google.with.Xamarin.Classes.csv"
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

        //-------------------------------------------------------------------------------------------------------------------
        public static
            ReadOnlyCollection
            //ReadOnlyMemory
                    <
                        (
                            string ClassName,
                            string AndroidSupportClass,
                            string AndroidXClass,
                            string AndroidSupportClassFullyQualified,
                            string AndroidXClassFullyQualified,
                            // formatting space
                            string PackageAndroidSupport,
                            string PackageAndroidX,
                            string ManagedNamespaceXamarinAndroidSupport,
                            string ManagedNamespaceXamarinAndroidX
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
                        string ClassName,
                        string AndroidSupportClass,
                        string AndroidXClass,
                        string AndroidSupportClassFullyQualified,
                        string AndroidXClassFullyQualified,
                        // formatting space
                        string PackageAndroidSupport,
                        string PackageAndroidX,
                        string ManagedNamespaceXamarinAndroidSupport,
                        string ManagedNamespaceXamarinAndroidX
                    )
                >
                Convert_ClassMappings(IEnumerable<string[]> untyped_data)
        {
             System.Diagnostics.Trace.WriteLine($"    converting csv to strongly typed data");
           //int n = untyped_data.Count();

            //for(int i = 0; i < n; i++)
            foreach(string[] row in untyped_data)
            {
                //string[] row = untyped_data[i];

                //TODO - benchmarking
                //Span<string> row_data = row.AsSpan();

                yield return
                        (
                            //ClassName: row_data[0],
                            //AndroidSupportClass: row_data[1],
                            //AndroidXClass: row_data[2],
                            //AndroidSupportClassFullyQualified: row_data[3],
                            //AndroidXClassFullyQualified: row_data[4],
                            //      formatting space
                            //PackageAndroidSupport: row_data[5],
                            //PackageAndroidX: row_data[6],
                            //ManagedNamespaceXamarinAndroidSupport: row_data[7],
                            //ManagedNamespaceXamarinAndroidX: row_data[8]

                            ClassName: row[0],
                            AndroidSupportClass: row[1],
                            AndroidXClass: row[2],
                            AndroidSupportClassFullyQualified: row[3],
                            AndroidXClassFullyQualified: row[4],
                                 // formatting space
                            PackageAndroidSupport: row[5],
                            PackageAndroidX: row[6],
                            ManagedNamespaceXamarinAndroidSupport: row[7],
                            ManagedNamespaceXamarinAndroidX: row[8]
                        );
            }
        }
        //-------------------------------------------------------------------------------------------------------------------


        protected AndroidXMigrator androidx_migrator = null;

        protected Mono.Cecil.AssemblyDefinition asm_def = null;
        protected Stopwatch timer = null;
        protected StringBuilder log = null;
        protected string replacement = null;

        public static void LoadMappingsClasses(string file)
        {
            System.Diagnostics.Trace.WriteLine($" reading csv = {file}");
            Core.Text.CharacterSeparatedValues csv = new Core.Text.CharacterSeparatedValues();
            string content = csv.LoadAsync(file).Result;

            System.Diagnostics.Trace.WriteLine($"    parsing csv...");
            IEnumerable<string[]> mapping = csv
                                            .ParseTemporaryImplementation()
                                            .ToList()
                                            ;
            IEnumerable
                    <
                        (
                            string ClassName,
                            string AndroidSupportClass,
                            string AndroidXClass,
                            string AndroidSupportClassFullyQualified,
                            string AndroidXClassFullyQualified,
                            // formatting space
                            string PackageAndroidSupport,
                            string PackageAndroidX,
                            string ManagedNamespaceXamarinAndroidSupport,
                            string ManagedNamespaceXamarinAndroidX
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

        private static ReadOnlyMemory<char> memory_android_support;
        private static ReadOnlyMemory<char> memory_androidx;

        private static Memory<Memory<char>> TypeNamesToSkip;

        protected static void Initialize()
        {
            System.Diagnostics.Trace.WriteLine($"    Initialize...");
            memory_android_support = string.Intern("Android.Support").AsMemory();
            memory_androidx = string.Intern("AndroidX").AsMemory();

            TypeNamesToSkip = 
                            new Memory<char>[]
                                    {
                                        string.Intern("<Module>").ToCharArray().AsMemory(),
                                        string.Intern("__TypeRegistrations").ToCharArray().AsMemory(),
                                    }
                                    .AsMemory();

            return;
        }

        // Verification 
        public static HashSet<string> AndroidSupportNotFoundInGoogle = new HashSet<string>();

        // Android Support for searching
        // sorted for BinarySearch
        private static Memory<string> ClassMappingsSortedProjected;

        private static Memory
                <
                    (
                        string ClassName,
                        string AndroidSupportClass,
                        string AndroidXClass,
                        string AndroidSupportClassFullyQualified,
                        string AndroidXClassFullyQualified,
                        // formatting space
                        string PackageAndroidSupport,
                        string PackageAndroidX,
                        string ManagedNamespaceXamarinAndroidSupport,
                        string ManagedNamespaceXamarinAndroidX
                    )
                > ClassMappingsSorted;


        private static void InitializePerformance()
        {
            ClassMappingsSorted = ClassMappings
                                            .Where(i => !string.IsNullOrEmpty(i.AndroidSupportClassFullyQualified))
                                            .OrderBy(i => i.AndroidSupportClassFullyQualified)
                                            .ToArray()
                                            .AsMemory()
                                            ;

            int n = ClassMappingsSorted.Length;

            ClassMappingsSortedProjected = ClassMappingsSorted
                                                    .Select(i => i.AndroidSupportClassFullyQualified)
                                                    ;

            // Test
            string classname = "Android.Support.CustomTabs.CustomTabsServiceConnection";
            int idx = ClassMappingsSortedProjected.Span.BinarySearch(classname);
            if( idx != 42 )
            {
                string msg =
                    "Android.Support sorted classnames changed"
                    + Environment.NewLine +
                    "Could be change in mappings or bindings!"
                    + Environment.NewLine +
                    "CHECK!!!!"
                    ;

                throw new InvalidDataException(msg);
            }

            return;
        }

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
            }
            else
            {
                r = ClassMappingsSorted.Span[index].AndroidXClassFullyQualified;    
            }
            return r;
        }

        protected Mono.Cecil.IAssemblyResolver CreateAssemblyResolver()
        {
            string VsInstallRoot = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\";
            string TargetFrameworkVerison = "v9.0";

            Mono.Cecil.DefaultAssemblyResolver resolver = null;
            resolver = new Mono.Cecil.DefaultAssemblyResolver();
            if (!string.IsNullOrEmpty(VsInstallRoot) && Directory.Exists(VsInstallRoot))
            {
                resolver.AddSearchDirectory(Path.Combine(
                    VsInstallRoot,
                    @"Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\" + TargetFrameworkVerison
                    ));
            }
            else
            {
                resolver.AddSearchDirectory(Path.Combine
                    (
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Reference Assemblies\Microsoft\Framework\MonoAndroid\" + TargetFrameworkVerison
                ));
            }

            return resolver;
        }

        public abstract void Migrate(ref long duration);
    }
}
