using System;
using System.IO;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        Mono.Cecil.AssemblyDefinition asm_def = null;

        private Mono.Cecil.IAssemblyResolver CreateAssemblyResolver()
        {
            string VsInstallRoot = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\";
            string TargetFrameworkVerison = "v9.0";

            Mono.Cecil.DefaultAssemblyResolver resolver = null;
            resolver = new Mono.Cecil.DefaultAssemblyResolver();
            if (!string.IsNullOrEmpty(VsInstallRoot) && Directory.Exists(VsInstallRoot))
            {
                resolver.AddSearchDirectory(Path.Combine(
                    VsInstallRoot,
                    @"Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\" + TargetFrameworkVerison
                    ));
            }
            else
            {
                resolver.AddSearchDirectory(Path.Combine
                    (
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Reference Assemblies\Microsoft\Framework\MonoAndroid\" + TargetFrameworkVerison
                ));
            }

            return resolver;
        }
    }
}
