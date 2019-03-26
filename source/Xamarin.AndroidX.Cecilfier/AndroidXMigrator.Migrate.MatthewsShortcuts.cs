using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Cecil;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        private void MigrateWithWithStringsMathewsShortcuts(ref long duration)
        {
            string msg = $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}-matthews-shortcuts-androidx-migrated";
            string alg = "matthew";

            int idx = this.PathAssemblyOutput.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            string asm = this.PathAssemblyOutput.Substring(idx, this.PathAssemblyOutput.Length - idx );

            if
                (
                    asm.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("Java.Interop.", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                duration = -1;

                return;
            }

            log = new System.Text.StringBuilder();
            timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            string i = Path.ChangeExtension(this.PathAssemblyInput, $"{alg}.dll");
            if (File.Exists(i))
            {
                File.Delete(i);
            }
            File.Copy(this.PathAssemblyInput, i);
            string o = Path.ChangeExtension(this.PathAssemblyOutput, $"{alg}.dll");
            if (File.Exists(o))
            {
                File.Delete(o);
            }
            File.Copy(i, o);

            string i_pdb = Path.ChangeExtension(this.PathAssemblyInput, $"{alg}.pdb");
            string o_pdb = Path.ChangeExtension(this.PathAssemblyOutput, $"{alg}.pdb");

            bool hasPdb = File.Exists(i_pdb);
            if (hasPdb)
            {
                if (File.Exists(o_pdb))
                {
                    File.Delete(o_pdb);
                }
                if (File.Exists(i_pdb))
                {
                    File.Delete(i_pdb);
                }
                File.Copy(i_pdb, o_pdb);
            }

            var readerParams = new ReaderParameters
			{
				ReadSymbols = hasPdb,
			};

            asm_def = Mono.Cecil.AssemblyDefinition.ReadAssembly
                                                        (
                                                            o,
                                                            new Mono.Cecil.ReaderParameters
                                                                {
                                                                    AssemblyResolver = CreateAssemblyResolver(),
                                                                    ReadWrite = true,
                                                                    //InMemory = true,
                                                                    ReadSymbols = hasPdb,
                                                                }
                                                        );

            Trace.WriteLine($"===================================================================================");
            Trace.WriteLine($"migrating assembly               = {this.PathAssemblyInput}");

            var csv = LoadMapping("mappings/API.Mappings.Merged.Google.with.Xamarin.Classes.csv");

            bool needsMigration = false;

            foreach (ModuleDefinition module in asm_def.Modules)
            {
                Trace.WriteLine($"--------------------------------------------------------------------------");
                Trace.WriteLine($"    migrating Module           = {module.Name}");
                //module.AssemblyReferences;

                foreach (var typeRef in module.GetTypes())
                {
                    //Console.WriteLine($" => Processing type reference '{typeRef.FullName}'...");

                    if (!csv.TryGetValue(typeRef.FullName, out var newName) || typeRef.FullName == newName.FN)
                        continue;

                    var old = typeRef.FullName;
                    typeRef.Namespace = newName.NS;
                    Console.WriteLine($"     => Mapped type '{old}' to '{typeRef.FullName}'.");

                    if (assemblyMappings.TryGetValue(typeRef.Scope.Name, out var newAssembly) && typeRef.Scope.Name != newAssembly)
                    {
                        Console.WriteLine($"     => Mapped assembly '{typeRef.Scope.Name}' to '{newAssembly}'.");
                        typeRef.Scope.Name = newAssembly;
                    }
                    else if (assemblyMappings.ContainsValue(typeRef.Scope.Name))
                    {
                        Console.WriteLine($"     => Already mapped assembly '{typeRef.Scope.Name}'.");
                    }
                    else
                    {
                        Console.WriteLine($"*** Potential error for assembly {typeRef.Scope.Name}' ***");
                    }

                    needsMigration = true;
                }

                foreach (var typeRef in module.GetTypeReferences())
                {
                    //Console.WriteLine($" => Processing type reference '{typeRef.FullName}'...");

                    if (!csv.TryGetValue(typeRef.FullName, out var newName) || typeRef.FullName == newName.FN)
                        continue;

                    var old = typeRef.FullName;
                    typeRef.Namespace = newName.NS;
                    Console.WriteLine($"     => Mapped type '{old}' to '{typeRef.FullName}'.");

                    if (assemblyMappings.TryGetValue(typeRef.Scope.Name, out var newAssembly) && typeRef.Scope.Name != newAssembly)
                    {
                        Console.WriteLine($"     => Mapped assembly '{typeRef.Scope.Name}' to '{newAssembly}'.");
                        typeRef.Scope.Name = newAssembly;
                    }
                    else if (assemblyMappings.ContainsValue(typeRef.Scope.Name))
                    {
                        Console.WriteLine($"     => Already mapped assembly '{typeRef.Scope.Name}'.");
                    }
                    else
                    {
                        Console.WriteLine($"*** Potential error for assembly {typeRef.Scope.Name}' ***");
                    }

                    needsMigration = true;
                }
            }

            AST.Assembly ast_assembly = null;

            AndroidXMigrator.AbstractSyntaxTree.Assemblies.Add(ast_assembly);
            timer.Stop();

            log.AppendLine($"{timer.ElapsedMilliseconds}ms");
            Trace.WriteLine($"{timer.ElapsedMilliseconds}ms");
            //Trace.WriteLine(log.ToString());


            File.WriteAllText
                (
                    Path.ChangeExtension(this.PathAssemblyInput, $"AbstractSyntaxTree.{msg}.json"),
                    Newtonsoft.Json.JsonConvert.SerializeObject
                    (
                        ast_assembly,
                        Newtonsoft.Json.Formatting.Indented,
                        new Newtonsoft.Json.JsonSerializerSettings()
                        {
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                        }
                    )
                );


            System.Diagnostics.Debug.WriteLine(log.ToString());
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.matthews-shortcuts.txt"), log.ToString());

            asm_def.Write();

            duration = timer.ElapsedMilliseconds;

            return;
        }

        private static Dictionary<string, (string NS, string T, string FN)> LoadMapping(string csvFile)
        {
            var root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var dic = new Dictionary<string, (string NS, string T, string FN)>();

            foreach (var line in File.ReadAllText(Path.Combine(root, csvFile)).Split('\r', '\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var split = line.Split(',');

                var support = split[(int)Columns.AndroidSupportClassFullyQualified];
                var ns = split[(int)Columns.ManagedNamespaceXamarinAndroidX];
                var androidx = split[(int)Columns.AndroidXClassFullyQualified];

                if (string.IsNullOrWhiteSpace(support) ||
                    string.IsNullOrWhiteSpace(support) ||
                    string.IsNullOrWhiteSpace(androidx))
                    continue;

                var t = androidx.Substring(ns.Length + 1);
                dic[support] = (NS: ns, T: t, FN: androidx);
            }

            return dic;
        }

        private static Dictionary<string, string> assemblyMappings = new Dictionary<string, string>
        {
            { "Xamarin.Android.Support.v7.AppCompat", "Xamarin.AndroidX.Appcompat.Appcompat" },
            { "Xamarin.Android.Support.Fragment", "Xamarin.AndroidX.Fragment.Fragment" },
            { "Xamarin.Android.Support.Compat", "Xamarin.AndroidX.Core.Core" },
            { "Xamarin.Android.Support.Core.UI", "Xamarin.AndroidX.Legacy.CoreUI" },
        };

        private enum Columns
        {
            ClassName,
            AndroidSupportClass,
            AndroidXClass,
            AndroidSupportClassFullyQualified,
            AndroidXClassFullyQualified,
            PackageAndroidSupport,
            PackageAndroidX,
            ManagedNamespaceXamarinAndroidSupport,
            ManagedNamespaceXamarinAndroidX
        }
    }
}
