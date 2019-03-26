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
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX,
                string TypenameFullyQualifiedXamarin
            )[]
                map_sorted = null;
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX,
                string TypenameFullyQualifiedXamarin
            )[]
                map_searched = null;

            string[] map_searched_index = null;

            int n_as = this.MappingsAndroidSupport.MappingsForMigrationMergeJoin.Count();
            int n_ax = this.MappingsAndroidSupport.MappingsForMigrationMergeJoin.Count();

            if (n_as > n_ax)
            {
                map_sorted = this.MappingsAndroidSupport
                                        .MappingsForMigrationMergeJoin
                                        .ToArray()
                                        ;
                map_searched = this.MappingsAndroidX
                                        .MappingsForMigrationMergeJoin
                                        .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                        .ToArray()
                                        ;
                map_searched_index = map_searched
                                        .Select(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                        .ToArray()
                                        ;
            }
            else
            {
                map_sorted = this.MappingsAndroidX
                                        .MappingsForMigrationMergeJoin
                                        .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidX)
                                        .ToArray()
                                        ;
                map_searched = this.MappingsAndroidSupport
                                        .MappingsForMigrationMergeJoin
                                        .OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                        .ToArray()
                                        ;
                map_searched_index = map_searched
                                        .Select(tuple => tuple.TypenameFullyQualifiedAndroidSupport)
                                        .ToArray()
                                        ;

                //File.WriteAllText("map_sorted.csv", string.Join(Environment.NewLine, map_sorted));
            }                                            

            foreach(var tuple_as in map_sorted)
            {
                int index = Array.BinarySearch(map_searched_index, tuple_as.TypenameFullyQualifiedAndroidX);

                string found_ax = null;
                string found_ax_xm = null;
                if (index >= 0 && index < map_sorted.Count())
                {
                    found_ax = map_sorted[index].TypenameFullyQualifiedAndroidX;
                    found_ax_xm = map_sorted[index].TypenameFullyQualifiedXamarin;
                }

                yield return
                    (
                        TypenameFullyQualifiedAndroidSupport: tuple_as.TypenameFullyQualifiedAndroidSupport,
                        TypenameFullyQualifiedAndroidX: found_ax,
                        TypenameFullyQualifiedXamarinAndroidSupport: tuple_as.TypenameFullyQualifiedXamarin,
                        TypenameFullyQualifiedXamarinAndroidX: found_ax_xm
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
