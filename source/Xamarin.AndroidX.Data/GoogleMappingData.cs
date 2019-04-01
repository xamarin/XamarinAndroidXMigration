using System.Collections.Generic;

namespace Xamarin.AndroidX.Data
{
    public abstract class GoogleMappingData
    {
        public GoogleMappingData()
        {
            this.FilterForSizeReduction = new string[]
            {
                "android.arch.paging",
                "android.arch.persistence.db",
                "android.arch.persistence.db.framework",
                "android.arch.persistence.room",
                "android.databinding",
                "android.support.test",
            };

            return;
        }

       public string[][] DataTable
        {
            get;
            set;
        }

        public
            IEnumerable
                <
                    (
                        string TypenameFullyQualifiedAndroidSupport,
                        string TypenameFullyQualifiedAndroidX
                    )
                >
                        Mapping
        {
            get;
            protected set;
        }

        public abstract void Initialize();

        public void OptimizeMappingsForClasses(string class_fully_qualified)
        {
        }

        public string[] FilterForSizeReduction
        {
            get;
            set;
        }

        public List<(string Filteredpackage, int Number)> ReducedWithFiltering
        {
            get;
            protected set;
        }

        public
                (
                    bool IsNested,
                    string Packagename,
                    string Typename
                )
                    AnalyzeClass(string class_fully_qualified)
        {
            (
                bool IsNested,
                string Packagename,
                string Typename
            )
                class_parsed;

            string type_name = null;
            string package_name = null;
            bool is_nested = false;

            for(int i = 0; i < class_fully_qualified.Length; i++)
            {
                char current = class_fully_qualified[i];
                if(current == '.')
                {
                    if ( char.IsUpper(class_fully_qualified[i + 1]))
                    {
                        type_name = class_fully_qualified.Substring(i + 1);
                        package_name = class_fully_qualified.Substring(0, i);
                        if (type_name.Contains("."))
                        {
                            is_nested = true;
                        }
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            class_parsed =
                (
                    IsNested: is_nested,
                    Packagename: package_name,
                    Typename: type_name
                );

            return class_parsed;
        }

    }
}
