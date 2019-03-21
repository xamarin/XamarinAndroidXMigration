using System;
using System.Collections.Generic;
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

        public void MergeJoin()
        {
            mapings_for_migration = MergeJoinEnumeratorBlock();

        this.MappingsForMigration = mapings_for_migration.ToArray();

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
                            MergeJoinEnumeratorBlock()
        {
            foreach
                (
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                        row in MappingsAndroidSupport.GoogleMappingsData.Mapping
                )
            {
                string tnfq_as = row.TypenameFullyQualifiedAndroidSupport;
                string tnfq_ax = row.TypenameFullyQualifiedAndroidX;

                string tn_xm_as_found = null;
                string tn_xm_ax_found = null;

                //Parallel.Invoke
                //    (
                //        () =>
                        //{
                            var array_as = MappingsAndroidSupport
                                            .MappingsForMigrationMergeJoin
                                            .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                            .ToArray()
                                            ;
                            var array_as_index = array_as
                                                    .Select(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                                    .ToArray()
                                                    ;
                            int index_as = Array.BinarySearch(array_as_index, tnfq_as);

                            if (index_as >= 0 && index_as < array_as.Length)
                            {
                                tn_xm_as_found = array_as[index_as].TypenameFullyQualifiedXamarin;
                            }
                        //},
                        //() =>
                        //{
                            var array_ax = MappingsAndroidX
                                            .MappingsForMigrationMergeJoin
                                            .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                            .ToArray()
                                            ;
                            var array_ax_index = array_ax
                                                    .Select(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                                    .ToArray()
                                                    ;
                            int index_ax = Array.BinarySearch(array_ax_index, tnfq_ax);

                            if (index_ax >= 0 && index_ax < array_ax.Length)
                            {
                                tn_xm_ax_found = array_ax[index_ax].TypenameFullyQualifiedXamarin;
                            }
                    //    }
                    //);

                (
                    string TypenameFullyQualifiedAndroidSupport,
                    string TypenameFullyQualifiedAndroidX,
                    string TypenameFullyQualifiedXamarinAndroidSupport,
                    string TypenameFullyQualifiedXamarinAndroidX
                )
                    mapping;

                mapping =
                        (
                            TypenameFullyQualifiedAndroidSupport: tnfq_as,
                            TypenameFullyQualifiedAndroidX: tnfq_ax,
                            TypenameFullyQualifiedXamarinAndroidSupport: tn_xm_as_found,
                            TypenameFullyQualifiedXamarinAndroidX: tn_xm_ax_found
                        );

                yield return mapping;
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
