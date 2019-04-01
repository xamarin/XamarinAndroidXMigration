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
                    mapping_full_ax_unmatched_removed;

        public void MergeJoin()
        {
            as_with_merged_ax = this.MergeJoinAndroidSupportWithAndroidX().ToArray();
            Trace.WriteLine($"   Android.Support with merged AndroidX)");
            Trace.WriteLine($"      n = {as_with_merged_ax.Count()})");
            ax_with_merged_as = this.MergeJoinAndroidXWithAndroidSupport().ToArray();
            Trace.WriteLine($"   AndroidX with merged Android.Support)");
            Trace.WriteLine($"      n = {ax_with_merged_as.Count()})");

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
            Trace.WriteLine($"  removing AndroidX types without matching in Android.Support");
            mapping_full_ax_unmatched_removed = MappingsForMigration
                                                    .Where
                                                        (
                                                            t =>
                                                            {
                                                                return
                                                                    !
                                                                        (
                                                                            string.IsNullOrEmpty(t.TypenameFullyQualifiedAndroidSupport)
                                                                            &&
                                                                            string.IsNullOrEmpty(t.TypenameFullyQualifiedAndroidX)
                                                                            &&
                                                                            string.IsNullOrEmpty(t.TypenameFullyQualifiedXamarinAndroidSupport)
                                                                        );
                                                            }
                                                        )
                                                        .ToArray()
                                                        ;
            Trace.WriteLine($"  n = {mapping_full_ax_unmatched_removed.Count()}");
            MappingsForMigration = mapping_full_ax_unmatched_removed.ToArray();
            
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
                            MergeJoinAndroidSupportWithAndroidX()
        {

            Trace.WriteLine($"          foreach loop over Android.Support");
            Trace.WriteLine($"          searching AndroidX");
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
                string tn_as_in_as = row_as.TypenameFullyQualifiedAndroidSupport;
                string tn_ax_in_as = row_as.TypenameFullyQualifiedAndroidX;
                string tnm_as = row_as.TypenameFullyQualifiedXamarin;

                var found =
                            (
                                from row_ax in this.MappingsAndroidX.MappingsForMigrationMergeJoin
                                    where
                                       !string.IsNullOrEmpty(tn_as_in_as)
                                       &&
                                       !string.IsNullOrEmpty(tn_ax_in_as)
                                       &&
                                       !string.IsNullOrEmpty(row_ax.TypenameFullyQualifiedAndroidSupport)
                                       &&
                                       !string.IsNullOrEmpty(row_ax.TypenameFullyQualifiedAndroidX)
                                       &&
                                       row_ax.TypenameFullyQualifiedAndroidSupport == tn_as_in_as
                                       &&
                                       row_ax.TypenameFullyQualifiedAndroidX == tn_ax_in_as
                                select row_ax
                            )
                            .Distinct()
                            .ToArray();

                // no looping - for debugging and perfomrance!!!
                (
                    string TypenameFullyQualifiedAndroidSupport,
                    string TypenameFullyQualifiedAndroidX,
                    string TypenameFullyQualifiedXamarinAndroidSupport,
                    string TypenameFullyQualifiedXamarinAndroidX
                )
                    mapping_single;
                int n_found = found.Length;
                switch (n_found)
                {
                    case 0:
                        Trace.WriteLine($"   mapping not found: {tn_as_in_as}");
                        break;
                    case 1:
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[0].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        break;
                    case 2:
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[0].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[1].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        break;
                    case 3:
                        mapping_single =
                               (
                                   TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                   // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                   // should be 158 with material cases
                                   TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                   TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                   TypenameFullyQualifiedXamarinAndroidX: found[0].TypenameFullyQualifiedXamarin
                               );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[1].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[2].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[2].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        break;
                    case 4:
                        mapping_single =
                               (
                                   TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                   // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                   // should be 158 with material cases
                                   TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                   TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                   TypenameFullyQualifiedXamarinAndroidX: found[0].TypenameFullyQualifiedXamarin
                               );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[1].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[2].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[2].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_as,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[3].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: tnm_as,
                                    TypenameFullyQualifiedXamarinAndroidX: found[3].TypenameFullyQualifiedXamarin
                                );
                        yield return mapping_single;
                        break;
                    default:
                        // >= 5
                        // just a stupid empirical test
                        throw new InvalidOperationException("Suspicious mappings");
                }
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
                            MergeJoinAndroidXWithAndroidSupport()
        {
            Trace.WriteLine($"Merging Android.Support mapping data into AndroidX");
            Trace.WriteLine($"          preparing data (sorting for binary sort)");

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
                string tn_as_in_ax = row_ax.TypenameFullyQualifiedAndroidSupport;
                string tn_ax_in_ax = row_ax.TypenameFullyQualifiedAndroidX;
                string tnm_ax = row_ax.TypenameFullyQualifiedXamarin;

                var found =
                            (
                                from row_as in this.MappingsAndroidSupport.MappingsForMigrationMergeJoin
                                    where
                                       ! string.IsNullOrEmpty(tn_as_in_ax)
                                       &&
                                       ! string.IsNullOrEmpty(tn_ax_in_ax)
                                       &&
                                       ! string.IsNullOrEmpty(row_as.TypenameFullyQualifiedAndroidSupport)
                                       &&
                                       ! string.IsNullOrEmpty(row_as.TypenameFullyQualifiedAndroidX)
                                       &&
                                       row_as.TypenameFullyQualifiedAndroidSupport == tn_as_in_ax
                                       &&
                                       row_as.TypenameFullyQualifiedAndroidX == tn_ax_in_ax
                                    select row_as
                            )
                            .Distinct()
                            .ToArray();

                // no looping - for debugging and perfomrance!!!
                (
                    string TypenameFullyQualifiedAndroidSupport,
                    string TypenameFullyQualifiedAndroidX,
                    string TypenameFullyQualifiedXamarinAndroidSupport,
                    string TypenameFullyQualifiedXamarinAndroidX
                )
                    mapping_single;
                int n_found = found.Length;
                switch (n_found)
                {
                    case 0:
                        Trace.WriteLine($"   mapping not found: {tn_ax_in_ax}");
                        break;
                    case 1:
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[0].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        break;
                    case 2:
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[0].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[1].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        break;
                    case 3:
                        mapping_single =
                               (
                                   TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                   // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                   // should be 158 with material cases
                                   TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[0].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                               );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[1].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[2].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[2].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        break;
                    case 4:
                        mapping_single =
                               (
                                   TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                   // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                   // should be 158 with material cases
                                   TypenameFullyQualifiedAndroidX: found[0].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[0].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                               );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[1].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[1].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[2].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[2].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        mapping_single =
                                (
                                    TypenameFullyQualifiedAndroidSupport: tn_as_in_ax,
                                    // breakpoint condition for tests: f.TypenameFullyQualifiedAndroidX.StartsWith("android.")
                                    // should be 158 with material cases
                                    TypenameFullyQualifiedAndroidX: found[3].TypenameFullyQualifiedAndroidX,
                                    TypenameFullyQualifiedXamarinAndroidSupport: found[3].TypenameFullyQualifiedXamarin,
                                    TypenameFullyQualifiedXamarinAndroidX: tnm_ax
                                );
                        yield return mapping_single;
                        break;
                    default:
                        // >= 5
                        // just a stupid empirical test
                        throw new InvalidOperationException("Suspicious mappings");
                }

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
