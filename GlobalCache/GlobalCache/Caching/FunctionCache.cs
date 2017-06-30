using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GlobalCache.Test")]
namespace GlobalCache.Caching
{
    /// <summary>
    /// A <seealso cref="APO.Business.Caching.BasicMemoryCache"/> implementation
    /// specifically for caching functions and expressions by their parameters.
    /// </summary>
    public class FunctionCache : BasicMemoryCache
    {
        internal FunctionCache(MemoryCache cache) : base(cache)
        {
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Caches an expression's results by parsing its arguments and return type as parameters.
        /// </summary>
        public T Get<T>(Expression<Func<T>> expression, CacheItemPolicy cacheItemPolicy = null)
        {
            var parameters = ExpressionValueExtractor.GetArgumentValues(expression);
            var cacheKey = GenerateCacheKey(typeof(T).FullName, parameters);
            var result = base.Get(cacheKey, () => expression.Compile().Invoke(), cacheItemPolicy);
            return result;
        }

        [DebuggerStepThrough]
        public T Get<T>(Func<T> function, params object[] keyParameters)
        {
            var cacheKey = GenerateCacheKey(typeof(T).FullName, keyParameters);
            return base.Get(cacheKey, function);
        }

        [DebuggerStepThrough]
        public T Get<T>(Func<T> function, CacheItemPolicy cacheItemPolicy, params object[] keyParameters)
        {
            var cacheKey = GenerateCacheKey(typeof(T).FullName, keyParameters);
            return base.Get(cacheKey, function, cacheItemPolicy);
        }

        private class ExpressionValueExtractor
        {
            public static object[] GetArgumentValues<T>(Expression<Func<T>> expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }

                if (expression.Body is ConstantExpression)
                {
                    return ExtractConstantExpression(expression.Body);
                }
                if (expression.Body is MethodCallExpression)
                {
                    return ExtractMethodCallExpression(expression);
                }
                if (expression.Body is InvocationExpression)
                {
                    return ExtractInvocationCallExpression(expression);
                }
                
                // any unaccounted scenarios should throw here
                return new object[] { expression.Compile().Invoke() };
            }

            private static MemberExpression ResolveMemberExpression(Expression expression)
            {
                if (expression is MemberExpression)
                {
                    return (MemberExpression)expression;
                }
                else if (expression is UnaryExpression)
                {
                    // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                    return (MemberExpression)((UnaryExpression)expression).Operand;
                }
                else
                {
                    throw new NotSupportedException(expression.ToString());
                }
            }

            private static object[] ExtractMethodCallExpression<T>(Expression<Func<T>> expression)
            {
                var body = (MethodCallExpression)expression.Body;
                var values = new List<object>(body.Arguments.Count);

                foreach (var argument in body.Arguments)
                {
                    object value;
                    if (argument is ConstantExpression)
                    {
                        value = ExtractConstantExpression(argument);
                    }
                    else
                    {
                        var exp = ResolveMemberExpression(argument);

                        value = GetValue(exp);
                    }

                    values.Add(value);
                }

                return values.ToArray();
            }

            private static object[] ExtractInvocationCallExpression<T>(Expression<Func<T>> expression)
            {
                return new object[] { expression.Compile().Invoke() };
            }

            private static object[] ExtractConstantExpression(Expression exp)
            {
                var e = (ConstantExpression)exp;
                return new[] { e.Value };
            }


            private static object GetValue(MemberExpression exp)
            {
                // expression is ConstantExpression or FieldExpression
                if (exp.Expression is ConstantExpression)
                {
                    return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
                }
                else if (exp.Expression is MemberExpression)
                {
                    return GetValue((MemberExpression)exp.Expression);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
