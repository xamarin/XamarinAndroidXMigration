using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class AbstractSyntaxTree
    {
        public AbstractSyntaxTree()
        {
            Assemblies = new List<Assembly>();

            return;
        }

        public List<Assembly> Assemblies
        {
            get;
            set;
        }


    }
}
