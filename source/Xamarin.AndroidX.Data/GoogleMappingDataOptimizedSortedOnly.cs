using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.AndroidX.Data
{
    public class GoogleMappingDataOptimizedSortedOnly : GoogleMappingData
    {
        public GoogleMappingDataOptimizedSortedOnly()
        {
        }

        public
            IEnumerable
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                >
                        MappingSorted
        {
            get;
            protected set;
        }

        private
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX
            )
                []
                    mapping_sorted = null;


        public IEnumerable<string> MappingSortedIndex                        
        {
            get;
            protected set;
        }

        private string[] mapping_sorted_index = null;

        public override void Initialize()
        {
            // Filter out

            // generate strongly typed mappings (Cast)
            // type is named tuple
            this.Mapping = Cast();

            // sort Mappings for search
            // search for Android.Supporty
            this.MappingSorted = this.Mapping.OrderBy(tuple => tuple.TypenameFullyQualifiedAndroidSupport);

            // index
            // single column for search
            this.MappingSortedIndex = this.MappingSorted.Select(tuple => tuple.TypenameFullyQualifiedAndroidSupport);


            // arrays for Array API - easier port to Memory<char>
            this.mapping_sorted = this.MappingSorted.ToArray();
            this.mapping_sorted_index = this.MappingSortedIndex.ToArray();

            return;
        }

        public void FilterForSizeRedcution()
        {
            IEnumerable
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                >
                        filtered = Mapping;

            ReducedWithFiltering = new List<(string Filteredpackage, int Number)>();

            foreach (string packagename_as in this.FilterForSizeReduction)
            {
                filtered = filtered.Where
                                        (
                                            tuple =>
                                            {
                                                return
                                                    !tuple.TypenameFullyQualifiedAndroidSupport
                                                            .StartsWith
                                                                    (
                                                                        packagename_as,
                                                                        StringComparison.InvariantCulture
                                                                    )
                                                ;
                                            }
                                        );

                ReducedWithFiltering.Add
                                        (
                                            (
                                                Filteredpackage: packagename_as,
                                                Number: filtered.Count()
                                            )
                                        );
            }

            return;
        }

        public
            IEnumerable
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                >
                        Cast()
        {
            foreach(string[] row in DataTable)
            {
                if(row.Length != 2)
                {
                    continue;
                }

                string as_class = row[0];
                string ax_class = row[1];

                yield return
                            (
                                TypenameFullyQualifiedAndroidSupport: as_class,
                                TypenameFullyQualifiedAndroidX: ax_class
                            ); 
            }
        }

        public
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX
            )
                Find(string android_support)
        {
            (
                string TypenameFullyQualifiedAndroidSupport,
                string TypenameFullyQualifiedAndroidX
            ) result;


            int index = System.Array.BinarySearch(mapping_sorted_index, android_support);

            if (index < 0 || index > mapping_sorted_index.Length - 1)
            {
                result =
                        (
                            TypenameFullyQualifiedAndroidSupport: "Not found",
                            TypenameFullyQualifiedAndroidX: "Not found"
                        );
            }
            else
            {
                result = mapping_sorted[index];
            }

            return result;
        }
    }
}
