using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace RevolutionaryStuff.Core;

public static class LinqHelpers
{
    public static class StandardMethodNames
    {
        public const string OrderBy = "OrderBy";
        public const string OrderByDescending = "OrderByDescending";
        public const string Where = "Where";
        public const string FirstOrDefault = "FirstOrDefault";
        public const string Select = "Select";
        public const string Skip = "Skip";
        public const string Take = "Take";
        public const string Concat = "Concat";
        public static string GetSortOrder(bool isAscending)
            => isAscending ? OrderBy : OrderByDescending;
    }

    public static Expression GenerateStringConcat(Expression left, Expression right)
    {
        return Expression.Add(left, right, typeof(string).GetMethod(StandardMethodNames.Concat, [typeof(object), typeof(object)]));
    }

    private static Expression NestedProperty(Expression arg, string fieldName)
    {
        var left = fieldName.LeftOf(".");
        var right = fieldName.RightOf(".").TrimOrNull();
        var leftExp = Expression.Property(arg, left);
        return right == null ? leftExp : NestedProperty(leftExp, right);
    }

    private static Expression NullCheckNestedProperty(Expression arg, string fieldName)
    {
        var left = fieldName.LeftOf(".");
        var right = fieldName.RightOf(".").TrimOrNull();
        if (right == null) return null;
        var leftNameExp = Expression.Property(arg, left);
        var leftExp = Expression.NotEqual(leftNameExp, Expression.Constant(null));
        var rightExp = NullCheckNestedProperty(leftNameExp, right);
        return rightExp == null ? leftExp : (Expression)Expression.AndAlso(leftExp, rightExp);
    }

    public static IQueryable<TSource> ApplyFilters<TSource>(this IQueryable<TSource> q, IEnumerable<KeyValuePair<string, string>> filters)
    {
        if (filters == null) return q;

        var mfilters = filters.ToMultipleValueDictionary(f => f.Key, f => f.Value);

        var argParam = Expression.Parameter(typeof(TSource), "s");

        var ands = new List<Expression>();
        foreach (var fieldName in mfilters.Keys)
        {
            var expressions = new List<Expression>();
            foreach (var val in mfilters[fieldName])
            {
                var nullCheckProperty = NullCheckNestedProperty(argParam, fieldName);
                var nameProperty = NestedProperty(argParam, fieldName);
                var val1 = Expression.Constant(val);
                Expression e1 = Expression.Equal(nameProperty, val1);
                if (nullCheckProperty != null)
                {
                    e1 = Expression.AndAlso(nullCheckProperty, e1);
                }
                expressions.Add(e1);
            }
            while (expressions.Count > 1)
            {
                expressions[0] = Expression.Or(expressions[0], expressions[1]);
                expressions.RemoveAt(1);
            }
            ands.Add(expressions[0]);
        }
        if (ands.Count > 0)
        {
            while (ands.Count > 1)
            {
                ands[0] = Expression.AndAlso(ands[0], ands[1]);
                ands.RemoveAt(1);
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(ands[0], argParam);

            q = q.Where(lambda);
        }

        return q;
    }

    /// <summary>
    /// Perform an ordering based on a field name given the field value is a whole number that can be mapped to an enumeration
    /// </summary>
    /// <typeparam name="T">The element being sorted</typeparam>
    /// <param name="q">The queryable</param>
    /// <param name="sortColumn">The name of the column to be sorted</param>
    /// <param name="enumType">An enumeration which serves as the type mapping.  Respects the DisplayAttribute</param>
    /// <param name="isAscending">When true, sort ascending else sort descending</param>
    /// <returns>The sorted input</returns>
    public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, Type enumType, bool isAscending = true)
    {
        var items = new List<Tuple<Enum, int, int?, string>>();
        foreach (var name in Enum.GetNames(enumType))
        {
            var e = (Enum)Enum.Parse(enumType, name);
            var nVal = Convert.ToInt32(e);
            var displayAttr = e.GetCustomAttributes<DisplayAttribute>().FirstOrDefault();
            var displayName = displayAttr?.Name ?? displayAttr?.ShortName ?? name?.ToString();
            items.Add(Tuple.Create(e, nVal, displayAttr?.Order, displayName));
        }
        items.Sort((a, b) =>
        {
            var ret = a.Item3.GetValueOrDefault(int.MaxValue).CompareTo(b.Item3.GetValueOrDefault(int.MaxValue));
            if (ret == 0)
            {
                ret = a.Item4.CompareTo(b.Item4);
            }
            return ret;
        });
        var d = new Dictionary<int, int>();
        var pos = 0;
        foreach (var item in items)
        {
            d[item.Item2] = pos++;
        }
        return OrderByField(q, sortColumn, d, isAscending);
    }

    /// <summary>
    /// Perform an ordering based on a field name given the field value is a whole number that can be mapped to an alternate value supplied by a dictionary
    /// </summary>
    /// <typeparam name="T">The element being sorted</typeparam>
    /// <typeparam name="TMappedVal">The type of the resulting mapped value</typeparam>
    /// <param name="q">The queryable</param>
    /// <param name="sortColumn">The name of the column to be sorted</param>
    /// <param name="sortPosByfieldVal">The dictionary that performs the mapping</param>
    /// <param name="isAscending">When true, sort ascending else sort descending</param>
    /// <returns>The sorted input</returns>
    public static IOrderedQueryable<T> OrderByField<T, TMappedVal>(this IQueryable<T> q, string sortColumn, IDictionary<int, TMappedVal> sortPosByfieldVal, bool isAscending = true) where TMappedVal : IComparable<TMappedVal>
    {
        Requires.Text(sortColumn);
        ArgumentNullException.ThrowIfNull(sortPosByfieldVal);
        Requires.Positive(sortPosByfieldVal.Count, nameof(sortPosByfieldVal.Count));

        var param = Expression.Parameter(typeof(T), "p");
        var prop = NestedProperty(param, sortColumn);

        var kvps = sortPosByfieldVal.ToList();
        var expr = (Expression)Expression.Constant(kvps.Last().Value);
        for (var z = kvps.Count - 2; z >= 0; --z)
        {
            var kvp = kvps[z];
            expr = Expression.Condition(
                Expression.Equal(prop, Expression.Constant(kvp.Key)),
                Expression.Constant(kvp.Value),
                expr);
        }

        var mce = Expression.Call(
            typeof(Queryable),
            StandardMethodNames.GetSortOrder(isAscending),
            [q.ElementType, typeof(TMappedVal)],
            q.Expression,
            Expression.Lambda<Func<T, TMappedVal>>(expr, [param])
            );
        return (IOrderedQueryable<T>)q.Provider.CreateQuery<T>(mce);
    }

    /// <summary>
    /// Perform an ordering based on a field name
    /// </summary>
    /// <typeparam name="T">The element being sorted</typeparam>
    /// <param name="q">The queryable</param>
    /// <param name="sortColumn">The name of the column to be sorted</param>
    /// <param name="isAscending">When true, sort ascending else sort descending</param>
    /// <returns></returns>
    /// <remarks>http://stackoverflow.com/questions/12284085/sort-using-linq-expressions-expression</remarks>
    public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, bool isAscending = true)
    {
        Requires.Text(sortColumn);

        //Requires.Match(RegexHelpers.Common.CSharpIdentifier, sortColumn, nameof(sortColumn));
        var param = Expression.Parameter(typeof(T), "p");
        var prop = NestedProperty(param, sortColumn);
        var exp = Expression.Lambda(prop, param);
        var types = new[] { q.ElementType, exp.Body.Type };
        var mce = Expression.Call(typeof(Queryable), StandardMethodNames.GetSortOrder(isAscending), types, q.Expression, exp);
        return (IOrderedQueryable<T>)q.Provider.CreateQuery<T>(mce);
    }

    public enum OrderByFieldUnmappedBehaviors
    {
        UpFront,
        InPlace,
        AtEnd,
    }

    public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, IEnumerable<string> orderedValues, bool isAscending = true)
    {
        var d = new Dictionary<string, string>();
        var mapped = "o";
        var cnt = 0;
        if (orderedValues != null)
        {
            foreach (var v in orderedValues)
            {
                d[v] = mapped;
                mapped += "o";
                ++cnt;
            }
        }
        return cnt > 0
            ? q.OrderByField(sortColumn, d, isAscending, OrderByFieldUnmappedBehaviors.AtEnd)
            : q.OrderByField(sortColumn, isAscending);
    }

    public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, IDictionary<string, string> valueMapper, bool isAscending = true, OrderByFieldUnmappedBehaviors unmappedValueBehavior = OrderByFieldUnmappedBehaviors.InPlace)
    {
        Requires.Text(sortColumn);
        valueMapper ??= new Dictionary<string, string>();

        var param = Expression.Parameter(typeof(T), "p");
        var prop = NestedProperty(param, sortColumn);

        var last = valueMapper.Values.OrderBy().LastOrDefault() ?? "";
        var expr = unmappedValueBehavior switch
        {
            OrderByFieldUnmappedBehaviors.UpFront => GenerateStringConcat(Expression.Constant("UpFront_a_"), prop),
            OrderByFieldUnmappedBehaviors.InPlace => prop,
            OrderByFieldUnmappedBehaviors.AtEnd => GenerateStringConcat(Expression.Constant(last + "_AtEnd_"), prop),
            _ => throw new UnexpectedSwitchValueException(unmappedValueBehavior),
        };
        foreach (var kvp in valueMapper)
        {
            Expression mapped = unmappedValueBehavior switch
            {
                OrderByFieldUnmappedBehaviors.UpFront => Expression.Constant("UpFront_b_" + kvp.Value),
                OrderByFieldUnmappedBehaviors.InPlace => Expression.Constant(kvp.Value),
                OrderByFieldUnmappedBehaviors.AtEnd => Expression.Constant(kvp.Value),
                _ => throw new UnexpectedSwitchValueException(unmappedValueBehavior),
            };
            expr = Expression.Condition(
                Expression.Equal(prop, Expression.Constant(kvp.Key)),
                mapped,
                expr);
        }

        var mce = Expression.Call(
            typeof(Queryable),
            StandardMethodNames.GetSortOrder(isAscending),
            [q.ElementType, typeof(string)],
            q.Expression,
            Expression.Lambda<Func<T, string>>(expr, [param])
            );
        return (IOrderedQueryable<T>)q.Provider.CreateQuery<T>(mce);
    }

    public static IList<MemberInfo> GetMembers(this LambdaExpression e)
    {
        var memberInfos = new List<MemberInfo>();

        var body = e.Body as MemberExpression;
Again:
        if (body == null)
        {
            var ubody = (UnaryExpression)e.Body;
            body = ubody.Operand as MemberExpression;
        }

        memberInfos.Add(body.Member);

        if (body.Expression != null)
        {
            body = body.Expression as MemberExpression;
            if (body != null)
            {
                goto Again;
            }
        }

        memberInfos.Reverse();
        return memberInfos;
    }

    public static IList<MemberInfo> GetMembers(this MemberExpression exp)
    {
        var memberInfos = new List<MemberInfo>();

        var body = exp;
Again:
        if (body == null)
        {
            throw new NotImplementedException();
            //                UnaryExpression ubody = (UnaryExpression)exp.Body;
            //               body = ubody.Operand as MemberExpression;
        }

        memberInfos.Add(body.Member);

        if (body.Expression != null)
        {
            body = body.Expression as MemberExpression;
            if (body != null)
            {
                goto Again;
            }
        }

        memberInfos.Reverse();
        return memberInfos;
    }

    public static string GetName<TModel, TResult>(this Expression<Func<TModel, TResult>> exp)
    {
        return exp.GetMembers().Last().Name;
    }

    public static string GetFullyQualifiedName<TModel, TResult>(this Expression<Func<TModel, TResult>> exp, string separator = ".")
    {
        return exp.GetMembers().ConvertAll(z => z.Name).Format(separator);
    }

    /// <remarks>As an extension method, this "infects" way too many items</remarks>
    public static T When<T>(T item, Func<T, bool> condition, T whenNot = default) where T : class
        => item != null && condition(item) ? item : whenNot;

    /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
    public class ReplaceParameterVisitor<TResult> : ExpressionVisitor
    {

        private readonly ParameterExpression Parameter;

        private readonly Expression Replacement;

        public ReplaceParameterVisitor(ParameterExpression parameter, Expression replacement)
        {
            Parameter = parameter;
            Replacement = replacement;
        }

        public Expression<TResult> Visit<T>(Expression<T> node)
        {
            var parameters = node.Parameters.Where(p => p != Parameter);
            return Expression.Lambda<TResult>(Visit(node.Body), parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == Parameter ? Replacement : base.VisitParameter(node);
        }
    }

    /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, right.WithParametersOf(left).Body), left.Parameters);
    }

    /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, right.WithParametersOf(left).Body), left.Parameters);
    }

    /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
    private static Expression<Func<TResult>> WithParametersOf<T, TResult>(this Expression<Func<T, TResult>> left, Expression<Func<T, TResult>> right)
    {
        return new ReplaceParameterVisitor<Func<TResult>>(left.Parameters[0], right.Parameters[0]).Visit(left);
    }
}
