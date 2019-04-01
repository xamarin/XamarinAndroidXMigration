using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Text;

namespace Xamarin.AndroidX.Mapper
{
    public class MappingsGoogle
    {
        protected static HttpClient client = null;

        static MappingsGoogle()
        {
            client = new HttpClient();

            MappingsUrlClasses = "https://developer.android.com/topic/libraries/support-library/downloads/androidx-class-mapping.csv";
            MappingsRawClasses = Download(MappingsUrlClasses);

            MappingsUrlArtifacts = "https://developer.android.com/topic/libraries/support-library/downloads/androidx-artifact-mapping.csv";
            MappingsRawArtifacts = Download(MappingsUrlArtifacts);

            return;
        }

        public MappingsGoogle()
        {
        }

        public static string Download(string url)
        {
            Trace.WriteLine($"Downloading...");
            Trace.WriteLine($"          {url}");

            string result = null;

            using (HttpResponseMessage response = client.GetAsync(url).Result)
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                result = content.ReadAsStringAsync().Result;
            }

            int index = url.LastIndexOf('/');
            string filename = url.Substring(index + 1, url.Length - index - 1);
            File.WriteAllText(filename, result);

            Trace.WriteLine($"  saved ...");
            Trace.WriteLine($"          to");
            Trace.WriteLine($"          {filename}");

            return result;
        }

        public static string MappingsUrlClasses
        {
            get;
            set;
        }

        public static string MappingsRawClasses
        {
            get;
            protected set;
        }

        public static string MappingsUrlArtifacts
        {
            get;
            set;
        }

        public static string MappingsRawArtifacts
        {
            get;
            protected set;
        }


        public static string[][] Parse()
        {
            CharacterSeparatedValues csv = new CharacterSeparatedValues()
            {
                Text = MappingsRawClasses
            };

            return csv.ParseTemporaryImplementation
                                                (
                                                    has_header: true
                                                )
                                                .ToArray();
        }


    }
}
