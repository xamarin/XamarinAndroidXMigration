using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Mono.Cecil;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public partial class AndroidXMigrator
    {
        private static
            List
                <
                    (
                        string AndroidSupportJNI,
                        string AndroidXJNI
                    )
                >
                MigratedJNI;

        public static AST.AbstractSyntaxTree AbstractSyntaxTree
        {
            get;
        }

        public static List
                            <
                                (
                                    string Assembly, 
                                    string Algorithm, // Matthew's Shortcuts, Redth's Original Patch
                                    long Duration
                                ) 
                            > PerformanceData
        {
            get;
            private set;
        }

        static partial void InitializeAnalysis()
        {
            MigratedJNI = new List
                                <
                                    (
                                        string AndroidSupportJNI,
                                        string AndroidXJNI
                                    )
                                >();

            return;
        }
    }
}
