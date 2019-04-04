using System;

namespace ManagedAarxercise.CodingPerversions
{
    public static class FragmentExtensions
    {
        public static string ToString(this AndroidX.Fragment.App.Fragment fragment)
        {
            return $"{fragment.ToString().ToUpper()}";
        }
    }
}
