using System;

namespace Core.Collections.Generic
{
    public static class Enumerable
    {
        public static bool IsSorted<T>
                                (
                                    this System.Collections.Generic.IEnumerable<T> source,
                                    System.Collections.Generic.IComparer<T> comparer = null
                                )
            where T: IComparable<T>
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            if (null == comparer)
            {
                comparer = System.Collections.Generic.Comparer<T>.Default;
            }

            if (null == comparer)
            {
                throw new ArgumentException("No default comparer found.");
            }

            T prior = default(T);
            bool first = true;

            foreach (T item in source)
            {
                if (!first && comparer.Compare(prior, item) > 0)
                {
                    return false;
                }

                first = false;
                prior = item;
            }

            return true;
        }

    }
}
