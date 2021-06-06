using System;
using System.Linq.Expressions;

namespace WoWMarketWatcher.API.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TOuter, TResult>> Apply<TOuter, TInner, TResult>(this Expression<Func<TOuter, TInner>> outer, Expression<Func<TInner, TResult>> inner)
        {
            if (outer == null)
            {
                throw new ArgumentNullException(nameof(outer));
            }

            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            return Expression.Lambda<Func<TOuter, TResult>>(inner.Body.ReplaceParameter(inner.Parameters[0], outer.Body), outer.Parameters);
        }

        public static Expression<Func<TOuter, TResult>> ApplyTo<TInner, TResult, TOuter>(this Expression<Func<TInner, TResult>> inner, Expression<Func<TOuter, TInner>> outer)
        {
            return outer.Apply(inner);
        }

        public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            return new ParameterReplacer(source, target).Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression source;
            private readonly Expression target;

            public ParameterReplacer(ParameterExpression source, Expression target)
            {
                this.source = source;
                this.target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == this.source ? this.target : node;
            }
        }
    }
}