using System;
using System.Collections.Generic;

namespace Core.Linq
{
    public static class Memory
    {
        #region Select
        public static Memory<TResult> Select<TSource, TResult>
                                                (
                                                    this Memory<TSource> source,
                                                    Func<TSource, TResult> projection
                                                )
        {
            return SelectImpl(source, projection);
        }

        private static Memory<TResult> SelectImpl<TSource, TResult>
                                                (
                                                    this Memory<TSource> source,
                                                    Func<TSource, TResult> projection
                                                )
        {
            int n = source.Length;
            List<TResult> result = new List<TResult>(n);

            for(int i = 0; i < n; i++) 
            {
                result.Add(projection(source.Span[i]));
            }

            return result.ToArray().AsMemory();
        }
        #endregion

        #region Empty
        public static Memory<T> Empty<T>()
        {
            return new Memory<T>();
        }
        #endregion

    }
}
