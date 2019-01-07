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
            Expression<Func<TElement, TValue, bool>> expression, IEnumerable<TValue> values)
        {
            return source.Where(BuildWhereOrExpression(expression, values));
        }

        private static Expression<Func<TElement, bool>> BuildWhereOrExpression<TElement, TValue>(
            Expression<Func<TElement, TValue, bool>> expression, IEnumerable<TValue> values)
        {
            if (expression == null) { throw new ArgumentNullException("expression"); }
            var elementParam = expression.Parameters.First();
            var equals = values.Select(value => (Expression)Expression.Invoke(expression, elementParam, Expression.Constant(value)));

            var body = equals.Aggregate((accumulate, equal) => Expression.Or(accumulate, equal));
            return Expression.Lambda<Func<TElement, bool>>(body, elementParam);
        }
    }
}
