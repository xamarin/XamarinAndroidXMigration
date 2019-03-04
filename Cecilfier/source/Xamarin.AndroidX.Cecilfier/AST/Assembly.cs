using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Assembly : Item
    {
        public Assembly()
        {
            Namespaces = new List<Namespace>();

            return;
        }

        public List<Namespace> Namespaces
        {
            get;
            set;
        }

    }
}
