using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;

namespace Xamarin.AndroidX.Cecilfier.App
{
    // http://docs.go-mono.com/index.aspx?link=N%3AMono.Options
    // https://stackoverflow.com/questions/4625714/ndesk-options-mono-options-parameter-with-multiple-key-value-pairs
    // http://www.ndesk.org/doc/ndesk-options/NDesk.Options/OptionSet.html#T:NDesk.Options.OptionSet:Docs
    // http://geekswithblogs.net/robz/archive/2009/11/22/command-line-parsing-with-mono.options.aspx
    // http://www.jprl.com/Blog/archive/development/mono/2008/Jan-07.html
    // https://www.reddit.com/r/dotnet/comments/a7k7q9/systemcommandline_by_the_net_team/
    public class Program
    {
        private static int Verbosity;

        public static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Break();

            Dictionary<string, string> current = null;
            Dictionary<string, string> option_exact_pairs = new Dictionary<string, string>();
            Dictionary<string, string> option_bulk_with_pattern = new Dictionary<string, string>();

            Mono.Options.OptionSet option_set = new Mono.Options.OptionSet()
            {
                {
                    "exact-pairs",
                    v =>
                    {
                        current = option_exact_pairs;
                    }
                },
                {
                    "bulk-with-pattern",
                    v =>
                    {
                        current = option_bulk_with_pattern;
                    }
                },
                {
                    "<>",
                    v =>
                    {
                        string[] values = v.Split (new[]{'=', ':'}, 2);
                        current.Add (values [0], values [1]);
                    }
                }
            };

            //var suite = new Mono.Options.CommandSet("suite-name")
            //{
            //     Use strings and option values, as with OptionSet

            //    "usage: suite-name COMMAND [OPTIONS]+",
            //    {
            //        "v:",
            //        "verbosity",
            //        (int? v) =>
            //        {
            //            Verbosity = v.HasValue ? v.Value : Verbosity+1
            //        }
            //    },
            //     Commands may also be specified

            //    new Mono.Options.Command ("command-name", "command help")
            //    {
            //        Options = new Mono.Options.OptionSet
            //        {
            //            /*...*/
            //        },
            //        Run = args_run =>
            //        {
            //            /*...*/
            //        },
            //    },
            //    new CommandSubclass ("aaa", "bbbb"),
            //};
            //suite.Run (new string[] { });

            List<string> args_parsed = option_set.Parse(args);

            Console.WriteLine($"Xamarin.AndroidX.Migration");
            Console.WriteLine($"    subcommand: exact-pairs");

            if (option_exact_pairs.Count() == 0)
            {
                option_exact_pairs.Add
                    (
                        "../../../"
                        +
                        "../../../../X/Xamarin.AndroidX.Test.Libraries/externals/Telerik_UI_for_Xamarin_2019_1_220_1_Trial/Binaries/Android/Telerik.Xamarin.Android.Chart.dll",
                        "/Project/tmp/"
                    );

            }
            foreach(KeyValuePair<string, string> kvp in option_exact_pairs)
            {
                string file = Path.GetFileName($"{kvp.Key}");
                string directory = Path.GetDirectoryName($"{kvp.Key}");
                Console.WriteLine($"        {kvp.Key} to {kvp.Value}");
                Console.WriteLine($"        to ");
                Console.WriteLine($"            {kvp.Value}");
                IEnumerable<string> files = Directory.EnumerateFiles(directory, file, SearchOption.AllDirectories);
                List<string> dlls = new List<string>(files)
                                                    .Where(x => ! x.Contains("linksrc"))
                                                    .Where(x => ! x.Contains("android/assets"))
                                                    .Where(x => ! x.Contains(".app/"))
                                                    .Where(x => ! x.Contains(".resources.dll"))
                                                    .ToList()
                                                    ;
                       
                AndroidXMigrator migrator = null;
                foreach (string dll in dlls)
                {
                    string msg = $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}-androidx-migrated";
                    migrator = new AndroidXMigrator(dll, dll.Replace(".dll", $".{msg}.dll"));
                    migrator.Migrate();
                }
            }

            Console.WriteLine($"    .....................................");
            Console.WriteLine($"    subcommand: bulk-with-pattern");
            foreach(KeyValuePair<string, string> kvp in option_bulk_with_pattern)
            {
                Console.WriteLine($"        {kvp.Key} to {kvp.Value}");
            }

            return;
        }
    }
}
