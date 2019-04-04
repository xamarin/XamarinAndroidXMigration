using System;

namespace ManagedAarxercise.CodingPerversions
{
    public static class FragmentExtensions
    {
        public static string ToString(this Android.Support.V4.App.Fragment fragment)
        {
            return $"{fragment.ToString().ToUpper()}";
        }
    }
}
