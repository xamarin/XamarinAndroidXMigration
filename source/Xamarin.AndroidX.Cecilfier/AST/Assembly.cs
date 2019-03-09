using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Assembly : Item
    {
        public Assembly()
        {
            Modules = new List<Module>();

            return;
        }

        public List<Module> Modules
        {
            get;
            set;
        }

    }
}
