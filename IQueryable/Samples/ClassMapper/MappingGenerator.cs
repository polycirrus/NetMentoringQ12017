using System;
using System.Linq.Expressions;

namespace ClassMapper
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));
            var mapFunction =
                Expression.Lambda<Func<TSource, TDestination>>(
                Expression.New(typeof(TDestination)),
                sourceParam
                );

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }
    }
}
