using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDOC.BLL
{
    public static class Extensions
    {
        public static int MaxOrDefault<T>(this IEnumerable<T> source, Func<T,int> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var enumerable = source as IList<T> ?? source.ToList();
            return enumerable.Any() ? PrimitiveMax(enumerable.Select(selector)) : 0;
        }

        private static T PrimitiveMax<T>(IEnumerable<T> source) where T : struct, IComparable<T>
        {
            using (var iterator = source.GetEnumerator())
            {
                var max = iterator.Current;
                while (iterator.MoveNext())
                {
                    var item = iterator.Current;
                    if (max.CompareTo(item) < 0)
                    {
                        max = item;
                    }
                }

                return max;
            }
        } 
    }
}