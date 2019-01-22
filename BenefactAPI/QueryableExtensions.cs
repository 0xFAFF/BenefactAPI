using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class LINQExpressionExtensions
    {
        public static Expression<Func<TElement, bool>> BinaryCombinator<TElement>(
            this IEnumerable<Expression<Func<TElement, bool>>> lambdas, Func<Expression, Expression, Expression> combinator)
        {
            var parameter = Expression.Parameter(typeof(TElement), "element");
            var constantReplacer = new ConstantReplacer();
            return Expression.Lambda<Func<TElement, bool>>(
                lambdas
                .Select(lambda => new ParameterReplacerVisitor(lambda.Parameters[0], parameter).VisitAndConvert(lambda.Body, "BinaryCombinator"))
                .Select(body => constantReplacer.VisitAndConvert(body, "BinaryCombinator"))
                .Aggregate(combinator), parameter);
        }
        class ConstantReplacer : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var member = node.Member;
                if (node.Expression.NodeType == ExpressionType.Constant && member.MemberType == MemberTypes.Field)
                {
                    var field = member.DeclaringType.GetField(member.Name);
                    return Expression.Convert(
                        Expression.Constant(field.GetValue(((ConstantExpression)node.Expression).Value)),
                        field.FieldType);
                }
                return base.VisitMember(node);
            }
        }
        class ParameterReplacerVisitor : ExpressionVisitor
        {
            ParameterExpression ReplaceWith;
            ParameterExpression ToReplace;
            public ParameterReplacerVisitor(ParameterExpression toReplace, ParameterExpression replaceWith)
            {
                ToReplace = toReplace;
                ReplaceWith = replaceWith;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == ToReplace ? ReplaceWith : base.VisitParameter(node);
            }
        }
        public static IEnumerable<Expression<Func<T, U>>> SelectExp<V, T, U>(this IEnumerable<V> vs, Func<V, Expression<Func<T, U>>> selector)
        {
            return vs.Select(selector);
        }
    }
}
