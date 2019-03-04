using System;
using System.Collections.Generic;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator.AST
{
    public class Type : Item
    {
        public Type()
        {
            Methods = new List<Method>();

            return;
        } 

       public List<Method> Methods
       {
            get;
            set;
        }
    }
}
