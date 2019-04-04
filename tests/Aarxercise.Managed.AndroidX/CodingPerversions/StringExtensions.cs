using System;

namespace ManagedAarxercise.CodingPerversions
{
    public static class StringExtensions
    {
        public static string ToString(this string text, AndroidX.Fragment.App.Fragment fragment)
        {
            return $"{text.ToLower()} - {fragment.ToString().ToUpper()}";
        }
    }
}
