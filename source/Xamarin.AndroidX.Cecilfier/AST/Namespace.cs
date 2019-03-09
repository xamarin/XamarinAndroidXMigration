using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Namespace : Item
    {
        public Namespace()
        {
            Types = new List<Type>();

            return;
        }

        public List<Type> Types
        {
            get;
            set;
        }

    }
}
