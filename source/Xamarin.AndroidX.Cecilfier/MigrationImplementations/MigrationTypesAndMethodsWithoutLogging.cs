using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;
using AST=HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST;

namespace Xamarin.AndroidX.Cecilfier.MigrationImplementations
{
    public class MigrationTypesAndMethodsWithoutLogging : MigrationImplementation
    {
        public MigrationTypesAndMethodsWithoutLogging(AndroidXMigrator migrator) : base(migrator)
        {
            androidx_migrator = migrator;

            return;
        }

        public override void Migrate(ref long duration)
        {
            string msg = $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}-androidx-migrated";

            int idx = this.PathAssemblyOutput.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            string asm = this.PathAssemblyOutput.Substring(idx, this.PathAssemblyOutput.Length - idx );

            if
                (
                    asm.StartsWith("System", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("Java.Interop", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                duration = -1;

                return;
            }

            log = new System.Text.StringBuilder();
            timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            if (File.Exists(this.PathAssemblyOutput))
            {
                File.Delete(this.PathAssemblyOutput);
            }
            File.Copy(this.PathAssemblyInput, this.PathAssemblyOutput);

            if(File.Exists(Path.ChangeExtension(this.PathAssemblyOutput, "pdb")))
            {
                File.Delete(Path.ChangeExtension(this.PathAssemblyOutput, "pdb"));
            }
            if (File.Exists(Path.ChangeExtension(this.PathAssemblyInput, "pdb")))
            {
                File.Copy(Path.ChangeExtension(this.PathAssemblyInput, "pdb"), Path.ChangeExtension(this.PathAssemblyOutput, "pdb"));
            }

            bool hasPdb = File.Exists(Path.ChangeExtension(this.PathAssemblyInput, "pdb"));

			var readerParams = new ReaderParameters
			{
				ReadSymbols = hasPdb,
			};

            asm_def = Mono.Cecil.AssemblyDefinition.ReadAssembly
                                                        (
                                                            this.PathAssemblyOutput,
                                                            new Mono.Cecil.ReaderParameters
                                                                {
                                                                    AssemblyResolver = CreateAssemblyResolver(),
                                                                    ReadWrite = true,
                                                                    //InMemory = true,
                                                                    ReadSymbols = hasPdb,
                                                                }
                                                        );

			var needsMigration = false;

			var csv = LoadMapping("mappings.csv");

            foreach (ModuleDefinition module in asm_def.Modules)
            {
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
                }

                foreach (TypeDefinition type in module.GetTypes())
                {
                    //Console.WriteLine($" => Processing type reference '{typeRef.FullName}'...");

                    if (!csv.TryGetValue(type.FullName, out var newName) || type.FullName == newName.FN)
                        continue;

                    var old = type.FullName;
                    type.Namespace = newName.NS;
                    Console.WriteLine($"     => Mapped type '{old}' to '{type.FullName}'.");

                    if (assemblyMappings.TryGetValue(type.Scope.Name, out var newAssembly) && type.Scope.Name != newAssembly)
                    {
                        Console.WriteLine($"     => Mapped assembly '{type.Scope.Name}' to '{newAssembly}'.");
                        type.Scope.Name = newAssembly;
                    }
                    else if (assemblyMappings.ContainsValue(type.Scope.Name))
                    {
                        Console.WriteLine($"     => Already mapped assembly '{type.Scope.Name}'.");
                    }
                    else
                    {
                        Console.WriteLine($"*** Potential error for assembly {type.Scope.Name}' ***");
                    }

                    foreach (MethodDefinition method in type.Methods)
                    {
                        ProcessMethod(method);
                    }
                }
                needsMigration = true;
            }

            string migrated = null;

			if (needsMigration)
			{
				var parameters = new WriterParameters
				{
					WriteSymbols = hasPdb
				};

				asm_def.Write(migrated + ".temp", parameters);
				asm_def.Dispose();

				File.Delete(migrated);

				if (hasPdb)
				{
					File.Delete(Path.ChangeExtension(migrated, "pdb"));
					File.Move(migrated + ".temp", migrated);
					File.Move(migrated + ".pdb", Path.ChangeExtension(migrated, "pdb"));
				}

				Console.WriteLine($"Migrated assembly to '{migrated}'.");
			}

            timer.Stop();

            log.AppendLine($"{timer.ElapsedMilliseconds}ms");
            System.Diagnostics.Trace.WriteLine($"{timer.ElapsedMilliseconds}ms");
            //System.Diagnostics.Trace.WriteLine(log.ToString());

            System.Diagnostics.Debug.WriteLine(log.ToString());
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), log.ToString());

            asm_def.Write();

            duration = timer.ElapsedMilliseconds;

            return;
        }

		private static Dictionary<string, (string NS, string T, string FN)> LoadMapping(string csvFile)
		{
            var root =
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                    //mc++ 2nd time Path.GetDirectoryName(typeof(Program).Assembly.Location)
                    ;

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

		private static Dictionary<string, string> assemblyMappings = new Dictionary<string, string>
		{
			{ "Xamarin.Android.Support.v7.AppCompat", "Xamarin.AndroidX.Appcompat.Appcompat" },
			{ "Xamarin.Android.Support.Fragment", "Xamarin.AndroidX.Fragment.Fragment" },
			{ "Xamarin.Android.Support.Compat", "Xamarin.AndroidX.Core.Core" },
			{ "Xamarin.Android.Support.Core.UI", "Xamarin.AndroidX.Legacy.CoreUI" },
		};




        private AST.Type ProcessTypeReference(TypeReference type)
        {
            AST.Type ast_type_base = null;

            if
                (
                    type == null
                    ||
                    ! (type?.FullName).StartsWith("Android.Support.", StringComparison.Ordinal)
                )
            {
                return ast_type_base;
            }

            System.Diagnostics.Trace.WriteLine($"        processing References - TypeReference");
            System.Diagnostics.Trace.WriteLine($"            Name        = {type.Name}");
            System.Diagnostics.Trace.WriteLine($"            FullName    = {type.FullName}");

            string type_fqn_old = type.FullName;

            string r = FindReplacingTypeFromMappings(type.FullName);
            int idx = r.LastIndexOf('.');
            type.Namespace = r.Substring(0, idx);
			type.Scope.Name = r.Substring(idx + 1, r.Length - idx - 1);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            log.AppendLine($"    BaseType: {type.FullName}");
            Console.ResetColor();

           ast_type_base = new AST.Type()
           {
               Name = type.Name,
               NameFullyQualified = type.FullName,
               NameFullyQualifiedOldMigratred = type_fqn_old
           };

            return ast_type_base;
        }

        private AST.Method ProcessMethod(MethodDefinition method)
        {
            AST.Method ast_method = null;

            string jni_signature = ProcessMethodJNISignature(method);

            AST.MethodBody ast_method_body = ProcessMethodBody(method.Body);


            return ast_method;
        }


        private string ProcessMethodJNISignature(MethodDefinition method)
        {
            string jni_signature = null;

            foreach (CustomAttribute attr in method.CustomAttributes)
            {
                if (attr.AttributeType.FullName.Equals("Android.Runtime.RegisterAttribute"))
                {
                    CustomAttributeArgument jniSigArg = attr.ConstructorArguments[1];

                    string registerAttrMethodName = attr.ConstructorArguments[0].Value.ToString();
                    string registerAttributeJniSig = jniSigArg.Value?.ToString();
                    object registerAttributeNewJniSig = ReplaceJniSignature(registerAttributeJniSig);

                    attr.ConstructorArguments[1] = new CustomAttributeArgument(jniSigArg.Type, registerAttributeNewJniSig);

                    bool isBindingMethod = true;

                    log.AppendLine($"[Register(\"{attr.ConstructorArguments[0].Value}\", \"{registerAttributeNewJniSig}\")]");
                }
            }

            return jni_signature;
        }

        private AST.MethodBody ProcessMethodBody(Mono.Cecil.Cil.MethodBody method_body)
        {
            AST.MethodBody ast_method_body = null;

            // Replace all the JNI Signatures inside the method body
            foreach (Mono.Cecil.Cil.Instruction instr in method_body.Instructions)
            {
                if (instr.OpCode.Name == "ldstr")
                {
                    string jniSig = instr.Operand.ToString();

                    int indexOfDot = jniSig.IndexOf('.');

                    if (indexOfDot < 0)
                    {
                        continue;
                    }

                    // New binding glue style is `methodName.(Lparamater/Type;)Lreturn/Type;`
                    if (indexOfDot >= 0)
                    {
                        string methodName = jniSig.Substring(0, indexOfDot);
                        string newJniSig = ReplaceJniSignature(jniSig.Substring(indexOfDot + 1));
                        instr.Operand = $"{methodName}.{newJniSig}";

                        log.AppendLine($"{methodName} -> {newJniSig}");
                    }
                    // Old style is two strings, one with method name and then `(Lparameter/Type;)Lreturn/Type;`
                    else if (jniSig.Contains('(') && jniSig.Contains(')'))
                    {
                        string methodName = instr.Previous.Operand.ToString();
                        string newJniSig = ReplaceJniSignature(jniSig);
                        instr.Operand = newJniSig;

                        log.AppendLine($"{methodName} -> {newJniSig}");
                    }
                    else
                    {
                        string msg = "Method Body Code Smell";
                        //throw new 
                    }

                    if (ast_method_body == null)
                    {
                        ast_method_body = new AST.MethodBody();
                    }
                }
            }

            return ast_method_body;
        }



        static string ReplaceJniSignature(string jniSignature)
        {
            if
                (
                    //-------------------------------
                    // WTF ??
                    jniSignature.Contains("Forms.Init(); prior to using it.") // WTF
                    ||
                    jniSignature.Contains("Init() before this")
                    ||
                    jniSignature.Contains("Init(); prior to using it.")
                    //-------------------------------
                    // iOS - picked during batch brute force Ceciling
                    ||
                    jniSignature.Contains("FinishedLaunching ()") // Xamarin.Forms.Platform.iOS.migrated.dll
                    //-------------------------------
                    ||
                    string.IsNullOrEmpty(jniSignature)
                    ||
                    ! jniSignature.Contains('(')
                    ||
                    ! jniSignature.Contains(')')
                )
            {
                return jniSignature;
            }

            var sig = new global::Xamarin.Android.Tools.Bytecode.MethodTypeSignature(jniSignature);

            var sb_newSig = new System.Text.StringBuilder();

            sb_newSig.Append("(");

            foreach (var p in sig.Parameters)
            {
                string mapped = "mapped"; //mappings[p];
                sb_newSig.Append($"L{mapped};" ?? p);               
            }

            sb_newSig.Append(")");

            sb_newSig.Append(sig.ReturnTypeSignature);

            string newSig = null;  // sb_newSig.ToString();

            return newSig;
        }

    }
}
