using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Mono.Cecil.Rocks;
using Mono.Cecil;
using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        System.Text.StringBuilder log = new System.Text.StringBuilder();

        Stopwatch timer = new Stopwatch();

        string replacement = null;

        private void MigrateWithWithStringsOriginalPatchByRedth()
        {
            string msg = $"androidx-migrated-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}";

            int idx = this.PathAssemblyOutput.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            string asm = this.PathAssemblyOutput.Substring(idx, this.PathAssemblyOutput.Length - idx );

            if
                (
                    asm.StartsWith("Java.Interop", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("System", StringComparison.InvariantCultureIgnoreCase)
                    ||
                    asm.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                return;
            }

            timer.Start();

            System.Threading.Tasks.Parallel.Invoke
                                                (
                                                    () =>
                                                    {
                                                        if (File.Exists(this.PathAssemblyOutput))
                                                        {
                                                            File.Delete(this.PathAssemblyOutput);
                                                        }
                                                        File.Copy
                                                                (
                                                                    this.PathAssemblyInput,
                                                                    this.PathAssemblyOutput
                                                                );
                                                    },
                                                    () =>
                                                    {
                                                        if(File.Exists(Path.ChangeExtension(this.PathAssemblyOutput, "pdb")))
                                                        {
                                                            File.Delete(Path.ChangeExtension(this.PathAssemblyOutput, "pdb"));
                                                        }
                                                        File.Copy
                                                               (
                                                                    Path.ChangeExtension(this.PathAssemblyInput, "pdb"),
                                                                    Path.ChangeExtension(this.PathAssemblyOutput, "pdb")
                                                                );
                                                    }
                                                );
 
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

            System.Diagnostics.Trace.WriteLine($"===================================================================================");
            System.Diagnostics.Trace.WriteLine($"migrating assembly               = {this.PathAssemblyInput}");

            AST.Assembly ast_assembly = new AST.Assembly()
            {
                Name = asm
            };

            foreach(ModuleDefinition module in asm_def.Modules)
            {
                System.Diagnostics.Trace.WriteLine($"--------------------------------------------------------------------------");
                System.Diagnostics.Trace.WriteLine($"    migrating Module           = {module.Name}");
                //module.AssemblyReferences;

                AST.Module ast_module = ProcessModule(module);

                if(ast_module != null)
                {
                    if (ast_assembly == null)
                    {
                        ast_assembly = new AST.Assembly()
                        {

                        };
                    }
                    ast_assembly.Modules.Add(ast_module);
                }
            }

            timer.Stop();

            AndroidXMigrator.AbstractSyntaxTree.Assemblies.Add(ast_assembly);

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

            log.AppendLine($"{timer.ElapsedMilliseconds}ms");

            System.Diagnostics.Debug.WriteLine(log.ToString());
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), log.ToString());

            asm_def.Write();

            return;
        }

        private Module ProcessModule(ModuleDefinition module)
        {
            AST.Module ast_module = null;

            foreach (TypeDefinition type in module.Types)
            {
                if
                    (
                        //type.FullName == "<Module>"
                        //||
                        //type.FullName == "<PrivateImplementationDetails>"
                        //||
                        type.FullName.StartsWith("AndroidX", StringComparison.Ordinal)
                        ||
                        type.FullName.StartsWith("Java.Interop", StringComparison.Ordinal)
                        ||
                        type.FullName.StartsWith("System", StringComparison.Ordinal)
                        ||
                        type.FullName.StartsWith("Microsoft", StringComparison.Ordinal)
                    )
                {
                    continue;
                }

                System.Diagnostics.Trace.WriteLine($"    processing Type");
                System.Diagnostics.Trace.WriteLine($"        Name        = {type.Name}");
                System.Diagnostics.Trace.WriteLine($"        FullName    = {type.FullName}");
                System.Diagnostics.Trace.WriteLine($"        IsClass     = {type.IsClass}");
                System.Diagnostics.Trace.WriteLine($"        IsInterface = {type.IsInterface}");

                AST.Type ast_type = ProcessType(type);

                if(ast_type == null)
                {
                    continue;
                }
                else
                {
                    if (ast_module == null)
                    {
                        ast_module = new AST.Module()
                        { 
                            Name = module.Name
                        };
                    }
                    ast_module.Types.Add(ast_type);
                }

            }

            return ast_module;
        }

        private AST.Type ProcessType(TypeDefinition type)
        {
            AST.Type ast_type = null;

            AST.Type ast_type_base = ProcessBaseType(type.BaseType);
            if(ast_type_base != null)
            {
                TypeDefinition type_found = type.Module.Types
                                                            .Where(t => t.FullName == ast_type_base.NameFullyQualifiedOldMigratred)
                                                            .FirstOrDefault();
            }

            List<AST.Type> ast_types_nested = null;
            foreach (TypeDefinition type_nested in type.NestedTypes)
            {
                AST.Type ast_type_nested = ProcessNestedType(type_nested);
            }

            List<AST.Method> ast_methods = null;
            foreach(var method in type.Methods)
            {
                AST.Method ast_method = ProcessMethod(method);

                if (ast_method != null)
                {
                    ast_methods = new List<AST.Method>();
                }
                else
                {
                    continue;
                }

                ast_methods.Add(ast_method);
            }

            if (ast_type_base == null && ast_methods == null)
            {
                return ast_type;
            }

            ast_type = new AST.Type()
            {
                Name = type.Name,
                NameFullyQualified = type.FullName,
            };

            if (ast_type_base != null)
            {
                ast_type.BaseType = ast_type_base;
            }
            if (ast_methods != null)
            {
                ast_type_base.Methods = ast_methods;
            }

            return ast_type;
        }

        private AST.Type ProcessBaseType(TypeReference type_base)
        {
            AST.Type ast_type_base = null;

            if
                (
                    type_base == null
                    ||
                    ! (type_base?.FullName).StartsWith("Android.Support", StringComparison.Ordinal)
                )
            {
                return ast_type_base;
            }

            System.Diagnostics.Trace.WriteLine($"        processing BaseType - TypeReference");
            System.Diagnostics.Trace.WriteLine($"            Name        = {type_base.Name}");
            System.Diagnostics.Trace.WriteLine($"            FullName    = {type_base.FullName}");

            string type_fqn_old = type_base.FullName;

            string r = FindReplacingTypeFromMappings(type_base.FullName);
            int idx = r.LastIndexOf('.');
            type_base.Namespace = r.Substring(0, idx);
			type_base.Scope.Name = r.Substring(idx + 1, r.Length - idx - 1);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            log.AppendLine($"    BaseType: {type_base.FullName}");
            Console.ResetColor();

           ast_type_base = new AST.Type()
           {
               Name = type_base.Name,
               NameFullyQualified = type_base.FullName,
               NameFullyQualifiedOldMigratred = type_fqn_old
           };

            return ast_type_base;
        }

        private AST.Type ProcessNestedType(TypeDefinition type_nested)
        {
            AST.Type ast_type_nested = null;

            if
                (
                    type_nested == null
                    ||
                    ! type_nested.FullName.StartsWith("Android.Support")
                    ||
                    type_nested.Name.Contains("<>c")  // anonymous methods, lambdas 
                    ||
                    type_nested.Name.Contains("<>c__DisplayClass")  // anonymous methods, lambdas 
                )
            {
                return ast_type_nested;
            }
            
            string type_nested_fqn_old = type_nested.FullName;
            string r = FindReplacingTypeFromMappings(type_nested.FullName);
            int idx = r.LastIndexOf('.');
            type_nested.Namespace = r.Substring(0, idx);
			type_nested.Scope.Name = r.Substring(idx + 1, r.Length - idx - 1);
            Console.ResetColor();

            ast_type_nested = new AST.Type()
            {
               Name = type_nested.Name,
               NameFullyQualified = type_nested.FullName,
               NameFullyQualifiedOldMigratred = type_nested_fqn_old
            };

            return ast_type_nested;
        }


        private AST.Method ProcessMethod(MethodDefinition method)
        {
            AST.Method ast_method = null;

            System.Diagnostics.Trace.WriteLine($"        processing method");
            System.Diagnostics.Trace.WriteLine($"           Name        = {method.Name}");
            System.Diagnostics.Trace.WriteLine($"           FullName    = {method.ReturnType.FullName}");

            AST.Type ast_method_type_return = ProcessMethodReturnType(method.ReturnType);

            string jni_signature = ProcessMethodJNISignature(method);

            List<AST.Parameter> ast_method_parameters = null;
            foreach (ParameterDefinition method_parameter in method.Parameters)
            {
                AST.Parameter ast_method_parameter = ProcessMethodParameter(method_parameter);

                if (ast_method_parameter != null)
                {
                    if(ast_method_parameters == null)
                    {
                        ast_method_parameters = new List<AST.Parameter>();
                    }
                }
                else
                {
                    continue;
                }
                ast_method_parameters.Add(ast_method_parameter);
            }

            AST.MethodBody ast_method_body = ProcessMethodBody(method.Body);

            if (ast_method_type_return == null && jni_signature == null && ast_method_body == null && ast_method_parameters == null)
            {
                return ast_method;
            }

            ast_method = new AST.Method();

            if (ast_method_type_return != null)
            {
                ast_method.ReturnType = ast_method_type_return;
            }

            if (ast_method_body != null)
            {
                ast_method.Body = ast_method_body;
            }

            if (ast_method_parameters != null)
            {
                ast_method.Parameters = ast_method_parameters;
            }

            return ast_method;
        }

        private AST.Type ProcessMethodReturnType(TypeReference type_return)
        {
            AST.Type ast_type_return = null;

            if (! type_return.FullName.StartsWith("Android.Support"))
            {
                return ast_type_return;
            }

            System.Diagnostics.Trace.WriteLine($"        changing return type");
            System.Diagnostics.Trace.WriteLine($"           Name    = {type_return.Name}");
            System.Diagnostics.Trace.WriteLine($"           FullName    = {type_return.FullName}");

            string r = FindReplacingTypeFromMappings(type_return.FullName);
            type_return.Namespace = replacement;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            log.AppendLine($"{type_return.Name} returns {type_return.FullName}");
            Console.ResetColor();

            return ast_type_return;
        }

        private AST.Parameter ProcessMethodParameter(ParameterDefinition method_parameter)
        {
            AST.Parameter ast_method_parameter = null;


            string r = FindReplacingTypeFromMappings(method_parameter.ParameterType.FullName);
            method_parameter.ParameterType.Namespace = r;

            ast_method_parameter = new AST.Parameter()
            {

            };

            return ast_method_parameter;
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
                        ast_method_body = new MethodBody();
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

        protected string FindReplacingTypeFromMappings(string typename)
        {
            string r = null;
            int index = ClassMappingsSortedProjected.Span.BinarySearch(typename);
            if (index < 0)
            {
                string msg = "Android.Support class not found in mappings";

                //throw new InvalidOperationException(msg);

                AndroidSupportNotFoundInGoogle.Add(typename);
            }
            else
            {
                r = ClassMappingsSorted.Span[index].AndroidXClassFullyQualified;    
            }

            return r;
        }

        private void MigrateRadeksSample()
        {
            var a = AssemblyDefinition.ReadAssembly ("a.dll", new ReaderParameters { ReadWrite = true, InMemory = true });
            var b = AssemblyDefinition.ReadAssembly ("b.dll", new ReaderParameters { ReadWrite = true, InMemory = true });
            var t = a.MainModule.GetType ("N1.Abc");
            t.Namespace = "N2";
            a.Write ("a.dll");
            b.Write ("b.dll");
        }
    }
}
