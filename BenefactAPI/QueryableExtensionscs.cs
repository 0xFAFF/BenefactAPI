using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class QueryableExtensionscs
    {
        public static IQueryable<TElement> WhereOr<TElement, TValue>(this IQueryable<TElement> source,
            IEnumerable<TValue> values, Expression<Func<TElement, TValue, bool>> expression)
            => source.WhereApplicator(values, expression, Expression.Or);
        public static IQueryable<TElement> WhereAnd<TElement, TValue>(this IQueryable<TElement> source,
            IEnumerable<TValue> values, Expression<Func<TElement, TValue, bool>> expression)
            => source.WhereApplicator(values, expression, Expression.And);
        static IQueryable<TElement> WhereApplicator<TElement, TValue>(this IQueryable<TElement> source,
            IEnumerable<TValue> values, Expression<Func<TElement, TValue, bool>> expression,
            Func<Expression, Expression, Expression> aggregator)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            var elementParam = expression.Parameters.First();
            var equals = values.Select(value => (Expression)Expression.Invoke(expression, elementParam, Expression.Constant(value)));
            var body = equals.Aggregate(aggregator);
            return source.Where(Expression.Lambda<Func<TElement, bool>>(body, elementParam));
        }
    }
}
