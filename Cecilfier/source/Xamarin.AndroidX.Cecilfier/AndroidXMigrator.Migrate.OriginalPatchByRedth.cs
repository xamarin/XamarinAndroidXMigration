using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Mono.Cecil.Rocks;
using Mono.Cecil;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        System.Text.StringBuilder log = new System.Text.StringBuilder();

        Stopwatch timer = new Stopwatch();

        AST.Assembly ast_assembly = null;
        AST.Namespace ast_namespace = null;
        AST.Type ast_type = null;
        AST.Method ast_method = null;
        AST.Parameter ast_parameter = null;

        string replacement = null;

        private void MigrateWithWithStringsOriginalPatchByRedth()
        {
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

            if (File.Exists(this.PathAssemblyOutput))
            {
                File.Delete(this.PathAssemblyOutput);
            }
            File.Copy(this.PathAssemblyInput, this.PathAssemblyOutput);

            asm_def = Mono.Cecil.AssemblyDefinition.ReadAssembly
                                                        (
                                                            this.PathAssemblyOutput,
                                                            new Mono.Cecil.ReaderParameters
                                                                {
                                                                    AssemblyResolver = CreateAssemblyResolver(),
                                                                    ReadWrite = true,
                                                                    //InMemory = true 
                                                                }
                                                        );
            var allTypes = asm_def.MainModule.GetAllTypes();

            System.Diagnostics.Trace.WriteLine($"===================================================================================");
            System.Diagnostics.Trace.WriteLine($" migrating assembly                       = {this.PathAssemblyInput}");
            System.Diagnostics.Trace.WriteLine($"                 types Mono.Cecil       # = {allTypes.Count()}");


            ast_assembly = new AST.Assembly
            {
                Name = asm
            };

            foreach (var t in allTypes)
            {
                if
                    (
                        t.FullName.StartsWith("Java.Interop")
                        ||
                        t.FullName.StartsWith("System")
                        ||
                        t.FullName.StartsWith("Microsoft")
                    )
                {
                    continue;
                }

                if(t.HasNestedTypes)
                {
                    ProcessNestedTypes(t);
                }
                //System.Diagnostics.Trace.WriteLine($"    processing Type");
                //System.Diagnostics.Trace.WriteLine($"       Name        = {t.Name}");
                //System.Diagnostics.Trace.WriteLine($"       FullName    = {t.FullName}");
                //System.Diagnostics.Trace.WriteLine($"       IsClass     = {t.IsClass}");
                //System.Diagnostics.Trace.WriteLine($"       IsInterface = {t.IsInterface}");

                var methods = t.GetMethods();

                foreach (var method in t.GetMethods())
                {
                    //System.Diagnostics.Trace.WriteLine($"        processing method");
                    //System.Diagnostics.Trace.WriteLine($"           Name        = {method.Name}");
                    //System.Diagnostics.Trace.WriteLine($"           FullName    = {method.ReturnType.FullName}");

                    var hasSupport = method.MethodReturnType?.ReturnType?.FullName?.StartsWith("Android.Support") ?? false;

                    if (hasSupport)
                    {

                        System.Diagnostics.Trace.WriteLine($"        changing return type");
                        System.Diagnostics.Trace.WriteLine($"           method.Name    = {method.Name}");
                        System.Diagnostics.Trace.WriteLine($"           ReturnType     = {method.MethodReturnType.ReturnType.Name}");
                        System.Diagnostics.Trace.WriteLine($"                          = {method.MethodReturnType.ReturnType.FullName}");

                        string classname = method.MethodReturnType.ReturnType.FullName;
                        int index = ClassMappingsSortedProjected.Span.BinarySearch(classname);
                        if (index < 0)
                        {
                            string msg = $"Android.Support class not found in mappings: {classname}";

                            //throw new InvalidOperationException(msg);

                            AndroidSupportNotFoundInGoogle.Add(classname);
                            continue;
                        }
                        replacement = ClassMappingsSorted.Span[index].AndroidXClassFullyQualified;
                        method.MethodReturnType.ReturnType.Namespace = replacement;
                        //replacement = method.MethodReturnType.ReturnType.FullName.Replace
                        //                                                            (
                        //                                                                "Android.Support",
                        //                                                                "AndroidX"
                        //                                                            );
                        //method.MethodReturnType.ReturnType.FullName = replacement;

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Beep();
                        log.AppendLine($"{method.Name} returns {method.ReturnType.FullName}");
                        Console.ResetColor();

                        ast_method = new AST.Method()
                        {
                            Name = method.Name,
                            NameFullyQualified = method.ReturnType.FullName
                        };
                        ast_type = new AST.Type()
                        {
                            Name = t.Name,
                            NameFullyQualified = t.FullName
                        };
                        ast_type.Methods.Add(ast_method);

                        ast_namespace = new AST.Namespace()
                        {
                            Name = t.Namespace
                        };
                        ast_namespace.Types.Add(ast_type);
                        ast_assembly.Namespaces.Add(ast_namespace);
                    }

                    if (method.HasParameters)
                    {
                        ProcessMethod(method, t);
                    }

                    string registerAttrMethodName = null;
                    string registerAttributeJniSig = null;
                    string registerAttributeNewJniSig = null;

                    var isBindingMethod = false;

                    foreach (var attr in method.CustomAttributes)
                    {
                        if (attr.AttributeType.FullName.Equals("Android.Runtime.RegisterAttribute"))
                        {
                            var jniSigArg = attr.ConstructorArguments[1];

                            registerAttrMethodName = attr.ConstructorArguments[0].Value.ToString();
                            registerAttributeJniSig = jniSigArg.Value?.ToString();

                            registerAttributeNewJniSig = ReplaceJniSignature(registerAttributeJniSig);

                            attr.ConstructorArguments[1] = new CustomAttributeArgument(jniSigArg.Type, registerAttributeNewJniSig);

                            isBindingMethod = true;

                            log.AppendLine($"[Register(\"{attr.ConstructorArguments[0].Value}\", \"{registerAttributeNewJniSig}\")]");
                        }
                    }

                    //if (!isBindingMethod)
                    //    return;

                    if (method.HasBody)
                    {
                        // Replace all the JNI Signatures inside the method body
                        foreach (var instr in method.Body.Instructions)
                        {
                            if (instr.OpCode.Name == "ldstr")
                            {
                                var jniSig = instr.Operand.ToString();

                                var indexOfDot = jniSig.IndexOf('.');

                                // New binding glue style is `methodName.(Lparamater/Type;)Lreturn/Type;`
                                if (indexOfDot >= 0)
                                {
                                    var methodName = jniSig.Substring(0, indexOfDot);
                                    var newJniSig = ReplaceJniSignature(jniSig.Substring(indexOfDot + 1));
                                    instr.Operand = $"{methodName}.{newJniSig}";

                                    log.AppendLine($"{methodName} -> {newJniSig}");
                                }
                                // Old style is two strings, one with method name and then `(Lparameter/Type;)Lreturn/Type;`
                                else if (jniSig.Contains('(') && jniSig.Contains(')'))
                                {
                                    //var methodName = instr.Previous.Operand.ToString();
                                    //var newJniSig = ReplaceJniSignature(jniSig);

                                    //instr.Operand = newJniSig;

                                    //log.AppendLine($"{methodName} -> {newJniSig}");
                                }
                            }
                        }
                    }
                }
            }

            timer.Stop();

            AndroidXMigrator.AbstractSyntaxTree.Assemblies.Add(ast_assembly);
            File.WriteAllText
                (
                    $"AbstractSyntaxTree.{ast_assembly.Name}.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(ast_assembly, Newtonsoft.Json.Formatting.Indented)
                );

            log.AppendLine($"{timer.ElapsedMilliseconds}ms");

            System.Diagnostics.Debug.WriteLine(log.ToString());
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), log.ToString());

            asm_def.Write();

            return;
        }

        private void ProcessMethod(MethodDefinition method, TypeDefinition t)
        {
            bool hasSupport = false;

            foreach (var methodParam in method.Parameters)
            {
                // Replace Managed Parameter types
                if (methodParam.ParameterType.Namespace.StartsWith("Android.Support"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Beep();
                    hasSupport = true;
                    log.AppendLine($"{method.Name} paramater {methodParam.ParameterType.FullName}");

                    string classname = methodParam.ParameterType.FullName;
                    int index = ClassMappingsSortedProjected.Span.BinarySearch(classname);
                    if (index < 0)
                    {
                        string msg = "Android.Support class not found in mappings";

                        //throw new InvalidOperationException(msg);

                        AndroidSupportNotFoundInGoogle.Add(classname);
                        continue;
                    }
                    replacement = ClassMappingsSorted.Span[index].AndroidXClassFullyQualified;

                    methodParam.ParameterType.Namespace = replacement;

                    Console.ResetColor();
                    ast_parameter = new AST.Parameter()
                    {
                        Name = methodParam.Name,
                        Type = new AST.Type()
                        {
                            Name = methodParam.ParameterType.Name,
                            NameFullyQualified = methodParam.ParameterType.FullName
                        }
                    };
                    ast_method = new AST.Method()
                    {
                        Name = method.Name,
                        NameFullyQualified = method.ReturnType.FullName
                    };
                    ast_method.Parameters.Add(ast_parameter);

                    ast_type = new AST.Type()
                    {
                        Name = t.Name,
                        NameFullyQualified = t.FullName
                    };
                    ast_type.Methods.Add(ast_method);

                    ast_namespace = new AST.Namespace()
                    {
                        Name = t.Namespace
                    };
                    ast_namespace.Types.Add(ast_type);
                    ast_assembly.Namespaces.Add(ast_namespace);
                }
            }
        }

        private void ProcessNestedTypes(TypeDefinition t)
        {
            foreach(TypeDefinition t_nested in t.NestedTypes)
            {
                if
                    (
                        t_nested.Name.Contains("/<>c__DisplayClass")  // anonymous methods, lambdas 
                    )
                {
                    continue;
                }
            }

            return;
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
