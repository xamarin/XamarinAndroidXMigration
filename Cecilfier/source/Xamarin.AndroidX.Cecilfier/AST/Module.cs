using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Module : Item
    {
        public Module()
        {
            Types = new List<Type>();

            return;
        }

        public string Name
        {
            get;
            set;
        }

        public List<Type> Types
        {
            get;
            set;
        }

    }
}
