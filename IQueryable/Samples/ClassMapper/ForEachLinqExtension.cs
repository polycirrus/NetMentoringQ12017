using System;
using System.Collections.Generic;

namespace ClassMapper
{
    public static class ForEachLinqExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
    }
}
