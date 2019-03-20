using System;
using System.Collections.Generic;

namespace Xamarin.AndroidX.Mapper
{
    public class ReportGenerator
    {
        public ReportGenerator()
        {
            MappingsGoogleWithXamarin = new List<MappingsXamarin>();

            return;
        }

        public List<MappingsXamarin> MappingsGoogleWithXamarin
        {
            get;
            private set;
        }
    }
}
