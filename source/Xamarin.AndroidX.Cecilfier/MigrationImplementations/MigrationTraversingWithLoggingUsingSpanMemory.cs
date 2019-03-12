using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;

using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;
using AST=HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST;

namespace Xamarin.AndroidX.Cecilfier.MigrationImplementations
{
    public class MigrationTraversingWithLoggingUsingSpanMemory : MigrationImplementation
    {
        public MigrationTraversingWithLoggingUsingSpanMemory(AndroidXMigrator migrator) : base(migrator)
        {
            androidx_migrator = migrator;

            return;
        }

        public override void Migrate(ref long duration)
        {
            return;
        }
    }
}
