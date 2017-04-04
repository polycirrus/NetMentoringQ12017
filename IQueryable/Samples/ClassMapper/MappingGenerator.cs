using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassMapper
{
    public class MappingGenerator
    {
        public static Mapper<TSource, TDestination> Generate<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(propInfo => propInfo.CanRead);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(propInfo => propInfo.CanWrite).ToList();
            var propertyMappings = new Dictionary<PropertyInfo, PropertyInfo>();
            foreach (var sourceProperty in sourceProperties)
            {
                var mappedProperty = destinationProperties.FirstOrDefault(destProp => destProp.Name == sourceProperty.Name &&
                    destProp.PropertyType.IsAssignableFrom(sourceProperty.PropertyType));
                if (mappedProperty != null)
                    propertyMappings.Add(sourceProperty, mappedProperty);
            }

            Expression<Action<TSource, TDestination>> copyingLambda = (source, destination) =>
                propertyMappings.ForEach(x => x.Value.SetValue(destination, x.Key.GetValue(source)));

            var sourceParam = Expression.Parameter(typeof(TSource));
            var destinationParam = Expression.Parameter(typeof(TDestination));
            var creationExpression = Expression.Assign(destinationParam, Expression.New(typeof(TDestination))); // destination = new TDestination()
            var copyingExpression = Expression.Invoke(copyingLambda, sourceParam, destinationParam);            // copyingLambda(destination)

            var mapFunctionBody = Expression.Block(     // TDestination mapFunction(TSource source)
                                                        // {
                new[] { destinationParam },             //     TDestination destination;
                creationExpression,                     //     destination = new TDestination();
                copyingExpression,                      //     copyingLambda(destination);
                destinationParam);                      //     return destination;
                                                        // }

            var mapFunction = Expression.Lambda<Func<TSource, TDestination>>(mapFunctionBody, sourceParam);
            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }
    }
}
