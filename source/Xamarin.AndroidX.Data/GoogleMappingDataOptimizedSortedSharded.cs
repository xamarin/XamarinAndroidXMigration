using System.Collections.Generic;
using System.Linq;

namespace Xamarin.AndroidX.Data
{
    public class GoogleMappingDataOptimizedSortedSharded : GoogleMappingData
    {
        public GoogleMappingDataOptimizedSortedSharded()
        {
            MappingTypenamesWithPackage = new List
                                                    <
                                                        (
                                                            string TypenameAndroidSupport,
                                                            string PackageAndroidSupport,
                                                            string TypenameAndroidX,
                                                            string PackageAndroidX
                                                        )
                                                    >();
            return;
        }

        public
            IEnumerable
                <
                    (
                        string TypenameAndroidSupport,
                        string PackagenameAndroidSupport,
                        string TypenameAndroidX,
                        string PackagenameAndroidX
                    )
                >
                        Mapping
        {
            get;
            protected set;
        }

        public
            IEnumerable
                <
                    (
                        string TypenameAndroidSupport,
                        string PackagenameAndroidSupport,
                        string TypenameAndroidX,
                        string PackagenameAndroidX
                    )
                >
                        MappingSorted
        {
            get;
            protected set;
        }

        public static List
                        <
                            (
                                string TypenameFullyQualifiedAndroidSupport,
                                string TypenameFullyQualifiedAndroidX
                            )
                        >
                                TypesAmbigious
        {
            get;
            private set;
        }


        public static HashSet<string> TypesAndroidSupport
        {
            get;
            private set;
        }

        public static HashSet<string> TypesAndroidX
        {
            get;
            private set;
        }

        public static List<string> AnalysisData
        {
            get;
            private set;
        }

        public override void Initialize()
        {
            // prepare objects for single pass analysis
            TypesAndroidSupport = new HashSet<string>();
            TypesAndroidX = new HashSet<string>();
            TypesAmbigious = new List
                                    <
                                        (
                                            string TypenameFullyQualifiedAndroidSupport,
                                            string TypenameFullyQualifiedAndroidX
                                        )
                                    >();

            this.Mapping = Cast();
            this.MappingSorted = this.Mapping
                                        .OrderBy(tuple => tuple.PackagenameAndroidSupport)
                                        .ThenBy(tuple => tuple.TypenameAndroidSupport)
                                        ;
            this.MappingTypenamesWithPackage = Cast().ToList();

            return;
        }

        public void Analyze()
        {
           AnalysisData = new List<string>();

            int n = Mapping.Count();
            int n_as = TypesAndroidSupport.Count;
            int n_ax = TypesAndroidX.Count;

            AnalysisData.Add($"Number of mappings (Google)                             = {n}");
            AnalysisData.Add($"Number of unique typenames for Android.Support (Google) = {n_as}");
            AnalysisData.Add($"Number of unique typenames for AndroidX        (Google) = {n_ax}");
            AnalysisData.Add($"Typename uniqueness for Android.Support (for search optimizations) = {n  == n_as}");
            AnalysisData.Add($"Typename uniqueness for AndroidX        (for search optimizations) = {n  == n_ax}");

            return;
        }

        public List
                        <
                            (
                                string TypenameAndroidSupport,
                                string PackageAndroidSupport,
                                string TypenameAndroidX,
                                string PackageAndroidX
                            )
                        >
                                MappingTypenamesWithPackage
        {
            get;
            set;
        }

        public
            IEnumerable
                <
                    (
                        string TypenameAndroidSupport,
                        string PackageAndroidSupport,
                        string TypenameAndroidX,
                        string PackageAndroidX
                    )
                >
                        Cast()
        {
            foreach(string[] row in DataTable)
            {
                if (row.Length != 2)
                {
                    continue;
                }

                string as_class = row[0];
                string ax_class = row[1];

                (
                    bool IsNested,
                    string Packagename,
                    string Typename
                )
                    as_class_parsed = AnalyzeClass(as_class);

                string as_typename = as_class_parsed.Typename;

                TypesAndroidSupport.Add(as_typename);

                (
                    bool IsNested,
                    string Packagename,
                    string Typename
                )
                    ax_class_parsed = AnalyzeClass(ax_class);

                string ax_typename = ax_class_parsed.Typename;

                TypesAndroidX.Add(ax_typename);

                if(as_typename != ax_typename)
                {
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                        type_ambigious;

                    type_ambigious =
                    (
                        TypenameFullyQualifiedAndroidSupport: as_class,
                        TypenameFullyQualifiedAndroidX: ax_class
                    );

                    TypesAmbigious.Add(type_ambigious);
                }


                yield return
                            (
                                TypenameAndroidSupport: as_class_parsed.Typename,
                                PackageAndroidSupport: as_class_parsed.Packagename,
                                TypenameAndroidX: ax_class_parsed.Typename,
                                PackageAndroidX: ax_class_parsed.Packagename
                            ); 
            }
        }



    }
}
