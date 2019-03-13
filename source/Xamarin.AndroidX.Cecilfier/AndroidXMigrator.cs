using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Linq;
using Xamarin.AndroidX.Cecilfier.MigrationImplementations;

[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("Tests.XUnit")]

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        MigrationImplementation implementation = null;

        static AndroidXMigrator()
        {
            AbstractSyntaxTree = new AST.AbstractSyntaxTree();

            return;
        }

        public AndroidXMigrator(string input, string output)
        {
            this.PathAssemblyInput = input;
            this.PathAssemblyOutput = output;

            return;
        }

        public string PathAssemblyInput
        {
            get;
            set;
        }

        public string PathAssemblyOutput
        {
            get;
            set;
        }

        partial void MigrateWithSpanMemory(ref long duration);

        public void Migrate(bool span_memory_implementation = false)
        {
            long duration = -1;

            if (span_memory_implementation)
            {
                implementation = new MigrationTraversingWithLoggingUsingSpanMemory(this);
                implementation.Migrate(ref duration); 
            }
            else
            {
                implementation = new MigrationTypesAndMethodsWithLogging(this);
                //implementation = new MigrationTraversingWithLogging(this);
                implementation.Migrate(ref duration); 
            }

            return;
        }

        public static AST.AbstractSyntaxTree AbstractSyntaxTree
        {
            get;
        }
    }
}
