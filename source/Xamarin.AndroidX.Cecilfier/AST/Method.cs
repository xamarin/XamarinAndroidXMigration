using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Method : Item
    {
        public Method()
        {
            Parameters = new List<Parameter>();

            return;
        }

        public Type ReturnType
        {
            get;
            set;
        }

        public MethodBody Body
        {
            get;
            set;
        }

        public List<Parameter> Parameters
        {
            get;
            set;
        }

    }
}
