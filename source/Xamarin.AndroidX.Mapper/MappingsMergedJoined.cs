using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xamarin.AndroidX.Mapper
{
    public class MappingsMergedJoined
    {
        public MappingsMergedJoined()
        {
        }

        public MappingsXamarin MappingsAndroidSupport
        {
            get;
            set;
        }

        public MappingsXamarin MappingsAndroidX
        {
            get;
            set;
        }

        public
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX,
                string TypenameFullyQualifiedXamarinAndroidSupport,
                string TypenameFullyQualifiedXamarinAndroidX
            )[]
                MappingsForMigration
        {
            get;
            private set;
        }

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                            mapings_for_migration
        {
            get;
            private set;
        }

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                    as_with_merged_ax;

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                    ax_with_merged_as;

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                    mapping_full;

        public void MergeJoin()
        {
            as_with_merged_ax = this.MergeJoinAndroidXIntoAndroidSupport().ToArray();
            ax_with_merged_as = this.MergeJoinAndroidSupportIntoAndroidX().ToArray();

            Trace.WriteLine($"Concat");
            Trace.WriteLine($"   .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)");
            Trace.WriteLine($"   .ThenBy(tuple => tuple.TypenameFullyQualifiedAndroidX)");
            Trace.WriteLine($"   .Distinct()");
            mapping_full = as_with_merged_ax
                                .Concat(ax_with_merged_as)
                                .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                .ThenBy(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                .Distinct()
                                .ToArray()
                                ;
            Trace.WriteLine($"MappingsForMigration");
            Trace.WriteLine($"  Done");
            MappingsForMigration = mapping_full.ToArray();
            Trace.WriteLine($"  n = {MappingsForMigration.Length}");
                        
            return;
        }

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                            MergeJoinAndroidXIntoAndroidSupport()
        {
            Trace.WriteLine($"Merging AndroidX mapping data into AndroidSupport");
            Trace.WriteLine($"          preparing data (sorting for binary sort)");

            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX,
                string TypenameFullyQualifiedXamarin
            )[]
                map_ax_sorted = null;

            map_ax_sorted = this.MappingsAndroidX.MappingsForMigrationMergeJoin
                                    .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                    .ToArray()
                                    ;

            string[] map_ax_sorted_index = map_ax_sorted
                                                .Select(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                                .ToArray()
                                                ;

            Trace.WriteLine($"          foreach loop over Android.Support");
            Trace.WriteLine($"          searching AndroidX");
            Trace.WriteLine($"               n = {map_ax_sorted_index.Length}");
            foreach
                (
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX,
                        string TypenameFullyQualifiedXamarin
                    )
                    row_as in this.MappingsAndroidSupport.MappingsForMigrationMergeJoin
                )
            {
                string tn_ax = row_as.TypenameFullyQualifiedAndroidX;
                int index_ax = Array.BinarySearch(map_ax_sorted_index, tn_ax);

                string tn_as = row_as.TypenameFullyQualifiedAndroidSupport;
                string tnm_as = row_as.TypenameFullyQualifiedXamarin;
                string tnm_ax = null;

                if (index_ax >= 0 && index_ax < map_ax_sorted_index.Length)
                {
                    string tn_ax_test = map_ax_sorted[index_ax].TypenameFullyQualifiedAndroidX;
                    string tn_as_test = map_ax_sorted[index_ax].TypenameFullyQualifiedAndroidSupport;
                    if
                        (
                            tn_ax != tn_ax_test
                            ||
                            tn_as != tn_as_test
                        )
                    {
                        // crap
                        string msg = null;
                    }

                    tnm_ax = map_ax_sorted[index_ax].TypenameFullyQualifiedXamarin;
                }
                yield return
                        (
                            TypenameFullyQualifiedAndroidSupport: tn_as,
                            TypenameFullyQualifiedAndroidX: tn_ax,
                            TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                            TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                        );
                
            }
        }

        protected
            IEnumerable
                    <
                        (
                            string TypenameFullyQualifiedAndroidSupport,
                            string TypenameFullyQualifiedAndroidX,
                            string TypenameFullyQualifiedXamarinAndroidSupport,
                            string TypenameFullyQualifiedXamarinAndroidX
                        )
                    >
                            MergeJoinAndroidSupportIntoAndroidX()
        {
            Trace.WriteLine($"Merging Android.Support mapping data into AndroidX");
            Trace.WriteLine($"          preparing data (sorting for binary sort)");

            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX,
                string TypenameFullyQualifiedXamarin
            )[]
                map_as_sorted = null;

            map_as_sorted = this.MappingsAndroidSupport.MappingsForMigrationMergeJoin
                                    .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                    .ToArray()
                                    ;

            string[] map_as_sorted_index = map_as_sorted
                                                .Select(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                                .ToArray()
                                                ;

            Trace.WriteLine($"          foreach loop over Androidx");
            Trace.WriteLine($"          searching Android.Support");
            Trace.WriteLine($"               n = {map_as_sorted_index.Length}");
            foreach
                (
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX,
                        string TypenameFullyQualifiedXamarin
                    )
                    row_ax in this.MappingsAndroidX.MappingsForMigrationMergeJoin
                )
            {
                string tn_as = row_ax.TypenameFullyQualifiedAndroidSupport;
                int index_as = Array.BinarySearch(map_as_sorted_index, tn_as);

                string tn_ax = row_ax.TypenameFullyQualifiedAndroidSupport;
                string tnm_ax = row_ax.TypenameFullyQualifiedXamarin;
                string tnm_as = null;

                if (index_as >= 0 && index_as < map_as_sorted_index.Length)
                {
                    string tn_as_test = map_as_sorted[index_as].TypenameFullyQualifiedAndroidSupport;
                    string tn_ax_test = map_as_sorted[index_as].TypenameFullyQualifiedAndroidX;
                    if
                        (
                            tn_ax != tn_ax_test
                            ||
                            tn_as != tn_as_test
                        )
                    {
                        // crap
                        string msg = null;
                    }

                    tnm_as = map_as_sorted[index_as].TypenameFullyQualifiedXamarin;
                }
                yield return
                        (
                            TypenameFullyQualifiedAndroidSupport: tn_as,
                            TypenameFullyQualifiedAndroidX: tn_ax,
                            TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                            TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                        );
                                
            }
        }

        public void Dump()
        {
            Parallel.Invoke
                (
                    () =>
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        string[] resource_names = assembly.GetManifestResourceNames();
                        string file = resource_names
                                        .Single
                                            (
                                                f =>
                                                {
                                                    return
                                                        f.EndsWith
                                                            (
                                                                "migration-mapping-analysis-report.md",
                                                                StringComparison.InvariantCulture
                                                            );
                                                }
                                            );

                        string report = null;
                        using (Stream stream = assembly.GetManifestResourceStream(file))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            report = reader.ReadToEnd();
                        }


                        File.WriteAllText("migration-mapping-analysis-report.md", report);
                    },
                    () =>
                    {
                        string text = string.Join(Environment.NewLine, this.MappingsForMigration);
                        text = text.Replace("(", "");
                        text = text.Replace(")", "");
                        text = text.Replace(" ", "");
                        File.WriteAllText("MigrationMappings.csv", text);
                    }
                );
        }
    }
}
