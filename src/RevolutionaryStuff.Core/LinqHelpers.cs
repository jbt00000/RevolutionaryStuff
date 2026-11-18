using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Core;

public static class LinqHelpers
{
    public delegate PropertyInfo PropertyAliasToPropertyInfoLookup(Type t, string name);

    private static PropertyInfo PropertyAliasToPropertyInfoLookupImp(Type t, string name, bool ignoreCase, bool useSystemTextJson, bool useClr)
    {
        if (useSystemTextJson)
        {
            foreach (var pi in t.GetPropertiesPublicInstanceRead())
            {
                var jpn = pi.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (0 == string.Compare(jpn?.Name, name, ignoreCase))
                {
                    return pi;
                }
            }
        }
        if (useClr)
        {
            foreach (var pi in t.GetPropertiesPublicInstanceRead())
            {
                if (0 == string.Compare(pi.Name, name, ignoreCase))
                {
                    return pi;
                }
            }
        }
        return null;
    }

    public static readonly PropertyAliasToPropertyInfoLookup ClrCaseInsensitivePropertyNameToPropertyInfoLookup
        = (Type t, string name) => PropertyAliasToPropertyInfoLookupImp(t, name, true, false, true);

    public static class StandardMethodNames
    {
        public const string OrderBy = "OrderBy";
        public const string OrderByDescending = "OrderByDescending";
        public const string ThenBy = "ThenBy";
        public const string ThenByDescending = "ThenByDescending";
        public const string Where = "Where";
        public const string FirstOrDefault = "FirstOrDefault";
        public const string Select = "Select";
        public const string Skip = "Skip";
        public const string Take = "Take";
        public const string Concat = "Concat";

        public static string GetSortOrder<T>(IQueryable<T> q, bool isAscending)
        {
            var expression = q.Expression;
            while (expression is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Queryable))
            {
                if (methodCall.Method.Name is OrderBy or OrderByDescending or ThenBy or ThenByDescending)
                {
                    return GetThenBy(isAscending);
                }

                if (methodCall.Method.Name is nameof(Queryable.Where) or nameof(Queryable.Select) or nameof(Queryable.Skip) or nameof(Queryable.Take))
                {
                    expression = methodCall.Arguments[0];
                    continue;
                }

                break;
            }

            return GetSortOrder(isAscending);
        }

        public static string GetSortOrder(bool isAscending)
            => isAscending ? OrderBy : OrderByDescending;
        public static string GetThenBy(bool isAscending)
            => isAscending ? ThenBy : ThenByDescending;
    }

    public static Expression GenerateStringConcat(Expression left, Expression right)
    {
        return Expression.Add(left, right, typeof(string).GetMethod(StandardMethodNames.Concat, new[] { typeof(object), typeof(object) }));
    }

    private static Expression NestedProperty(Expression arg, string fieldName, PropertyAliasToPropertyInfoLookup lookup)
    {
        lookup ??= ClrCaseInsensitivePropertyNameToPropertyInfoLookup;
        var left = fieldName.LeftOf(".");
        var right = fieldName.RightOf(".").TrimOrNull();
        var pi = lookup(arg.Type, left);
        var leftExp = Expression.Property(arg, pi);
        return right == null ? leftExp : NestedProperty(leftExp, right, lookup);
    }

    private static Expression NullCheckNestedProperty(Expression arg, string fieldName, PropertyAliasToPropertyInfoLookup lookup)
    {
        lookup ??= ClrCaseInsensitivePropertyNameToPropertyInfoLookup;
        var left = fieldName.LeftOf(".");
        var right = fieldName.RightOf(".").TrimOrNull();
        if (right == null) return null;
        var leftNameExp = Expression.Property(arg, left);
        var leftExp = Expression.NotEqual(leftNameExp, Expression.Constant(null));
        var rightExp = NullCheckNestedProperty(leftNameExp, right, lookup);
        return rightExp == null ? leftExp : Expression.AndAlso(leftExp, rightExp);
    }

    extension<TSource>(IQueryable<TSource> sources)
    {
        public IQueryable<TSource> ApplyFilters(IEnumerable<KeyValuePair<string, string>> filters, PropertyAliasToPropertyInfoLookup lookup = null)
        {
            if (filters == null) return sources;

            var mfilters = filters.ToMultipleValueDictionary(f => f.Key, f => f.Value);

            var argParam = Expression.Parameter(typeof(TSource), "s");

            var ands = new List<Expression>();
            foreach (var fieldName in mfilters.Keys)
            {
                var expressions = new List<Expression>();
                foreach (var val in mfilters[fieldName])
                {
                    var nullCheckProperty = NullCheckNestedProperty(argParam, fieldName, lookup);
                    var nameProperty = NestedProperty(argParam, fieldName, lookup);
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

                sources = sources.Where(lambda);
            }

            return sources;
        }
    }

    extension<T>(IQueryable<T> sources)
    {
        /// <summary>
        /// Perform an ordering based on a field name given the field value is a whole number that can be mapped to an enumeration
        /// </summary>
        /// <typeparam name="T">The element being sorted</typeparam>
        /// <param name="sources">The queryable</param>
        /// <param name="sortColumn">The name of the column to be sorted</param>
        /// <param name="enumType">An enumeration which serves as the type mapping.  Respects the DisplayAttribute</param>
        /// <param name="isAscending">When true, sort ascending else sort descending</param>
        /// <returns>The sorted input</returns>
        public IOrderedQueryable<T> OrderByField(string sortColumn, Type enumType, bool isAscending = true, PropertyAliasToPropertyInfoLookup lookup = null)
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
            return OrderByField(sources, sortColumn, d, isAscending, lookup: lookup);
        }
        /// <summary>
        /// Perform an ordering based on a field name given the field value is a whole number that can be mapped to an alternate value supplied by a dictionary
        /// </summary>
        /// <typeparam name="T">The element being sorted</typeparam>
        /// <typeparam name="TMappedVal">The type of the resulting mapped value</typeparam>
        /// <param name="sources">The queryable</param>
        /// <param name="sortColumn">The name of the column to be sorted</param>
        /// <param name="sortPosByfieldVal">The dictionary that performs the mapping</param>
        /// <param name="isAscending">When true, sort ascending else sort descending</param>
        /// <returns>The sorted input</returns>
        public IOrderedQueryable<T> OrderByField<TMappedVal>(string sortColumn, IDictionary<int, TMappedVal> sortPosByfieldVal, bool isAscending = true, PropertyAliasToPropertyInfoLookup lookup = null) where TMappedVal : IComparable<TMappedVal>
        {
            Requires.Text(sortColumn);
            ArgumentNullException.ThrowIfNull(sortPosByfieldVal);
            Requires.Positive(sortPosByfieldVal.Count, nameof(sortPosByfieldVal.Count));

            var param = Expression.Parameter(typeof(T), "p");
            var prop = NestedProperty(param, sortColumn, lookup);

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

            var lambda = Expression.Lambda(expr, param);
            var types = new[] { sources.ElementType, typeof(TMappedVal) };
            var mce = Expression.Call(typeof(Queryable), StandardMethodNames.GetSortOrder(sources, isAscending), types, sources.Expression, lambda);
            return (IOrderedQueryable<T>)sources.Provider.CreateQuery<T>(mce);
        }
        /// <summary>
        /// Perform an ordering based on a field name
        /// </summary>
        /// <typeparam name="T">The element being sorted</typeparam>
        /// <param name="sources">The queryable</param>
        /// <param name="sortColumn">The name of the column to be sorted</param>
        /// <param name="isAscending">When true, sort ascending else sort descending</param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/12284085/sort-using-linq-expressions-expression</remarks>
        public IOrderedQueryable<T> OrderByField(string sortColumn, bool isAscending = true, PropertyAliasToPropertyInfoLookup lookup = null)
            => sources.SortByField(sortColumn, StandardMethodNames.GetSortOrder(isAscending), lookup);
        private IOrderedQueryable<T> SortByField(string sortColumn, string sortMethodName, PropertyAliasToPropertyInfoLookup lookup = null)
        {
            Requires.Text(sortColumn);

            //Requires.Match(RegexHelpers.Common.CSharpIdentifier, sortColumn, nameof(sortColumn));
            var param = Expression.Parameter(typeof(T), "p");
            var prop = NestedProperty(param, sortColumn, lookup);
            var exp = Expression.Lambda(prop, param);
            var types = new[] { sources.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), sortMethodName, types, sources.Expression, exp);
            return (IOrderedQueryable<T>)sources.Provider.CreateQuery<T>(mce);
        }
        public IOrderedQueryable<T> OrderByField(string sortColumn, IEnumerable<string> orderedValues, bool isAscending = true)
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
                ? sources.OrderByField(sortColumn, d, isAscending, OrderByFieldUnmappedBehaviorEnum.AtEnd)
                : sources.OrderByField(sortColumn, isAscending);
        }
        public IOrderedQueryable<T> OrderByField(string sortColumn, IDictionary<string, string> valueMapper, bool isAscending = true, OrderByFieldUnmappedBehaviorEnum unmappedValueBehavior = OrderByFieldUnmappedBehaviorEnum.InPlace, PropertyAliasToPropertyInfoLookup lookup = null)
        {
            Requires.Text(sortColumn);
            valueMapper ??= new Dictionary<string, string>();

            var param = Expression.Parameter(typeof(T), "p");
            var prop = NestedProperty(param, sortColumn, lookup);

            var last = valueMapper.Values.OrderBy().LastOrDefault() ?? "";
            var expr = unmappedValueBehavior switch
            {
                OrderByFieldUnmappedBehaviorEnum.UpFront => GenerateStringConcat(Expression.Constant("UpFront_a_"), prop),
                OrderByFieldUnmappedBehaviorEnum.InPlace => prop,
                OrderByFieldUnmappedBehaviorEnum.AtEnd => GenerateStringConcat(Expression.Constant(last + "_AtEnd_"), prop),
                _ => throw new UnexpectedSwitchValueException(unmappedValueBehavior),
            };
            foreach (var kvp in valueMapper)
            {
                Expression mapped = unmappedValueBehavior switch
                {
                    OrderByFieldUnmappedBehaviorEnum.UpFront => Expression.Constant("UpFront_b_" + kvp.Value),
                    OrderByFieldUnmappedBehaviorEnum.InPlace => Expression.Constant(kvp.Value),
                    OrderByFieldUnmappedBehaviorEnum.AtEnd => Expression.Constant(kvp.Value),
                    _ => throw new UnexpectedSwitchValueException(unmappedValueBehavior),
                };
                expr = Expression.Condition(
                    Expression.Equal(prop, Expression.Constant(kvp.Key)),
                    mapped,
                    expr);
            }

            var lambda = Expression.Lambda<Func<T, string>>(expr, param);
            var mce = Expression.Call(
                typeof(Queryable),
                StandardMethodNames.GetSortOrder(sources, isAscending),
                new[] { typeof(T), typeof(string) },
                sources.Expression,
                lambda
                );
            return (IOrderedQueryable<T>)sources.Provider.CreateQuery<T>(mce);
        }
    }





    extension<T>(IOrderedQueryable<T> sources)
    {
        public IOrderedQueryable<T> ThenByField(string sortColumn, bool isAscending = true, PropertyAliasToPropertyInfoLookup lookup = null)
                => sources.SortByField(sortColumn, StandardMethodNames.GetThenBy(isAscending), lookup);
    }



    public enum OrderByFieldUnmappedBehaviorEnum
    {
        UpFront,
        InPlace,
        AtEnd,
    }





    extension(LambdaExpression source)
    {
        public IList<MemberInfo> GetMembers()
        {
            var memberInfos = new List<MemberInfo>();

            var body = source.Body as MemberExpression;
Again:
            if (body == null)
            {
                var ubody = (UnaryExpression)source.Body;
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
    }

    extension(MemberExpression source)
    {
        public IList<MemberInfo> GetMembers()
        {
            var memberInfos = new List<MemberInfo>();

            var body = source;
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
    }

    private static string GetMemberNameFromName(MemberInfo mi)
        => mi.Name;

    extension<TModel, TResult>(Expression<Func<TModel, TResult>> source)
    {
        public string GetName(Func<MemberInfo, string> getMemberName = null)
                => (getMemberName ?? GetMemberNameFromName)(source.GetMembers().Last());
        public string GetFullyQualifiedName(Func<MemberInfo, string> getMemberName = null, string separator = ".")
                => source.GetMembers().ConvertAll(mi => (getMemberName ?? GetMemberNameFromName)(mi)).Format(separator);
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

    extension<T>(Expression<Func<T, bool>> source)
    {
        /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
        public Expression<Func<T, bool>> And(Expression<Func<T, bool>> right)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(source.Body, right.WithParametersOf(source).Body), source.Parameters);
        }
        /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
        public Expression<Func<T, bool>> Or(Expression<Func<T, bool>> right)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(source.Body, right.WithParametersOf(source).Body), source.Parameters);
        }
    }



    public static Expression<Func<T, bool>> Or<T>(IList<Expression<Func<T, bool>>> conditions)
    {
        return conditions == null || conditions.Count == 0
            ? (x => false)
            : conditions.Count == 1 ? conditions[0] : Or(conditions[0], Or(conditions.Skip(1).ToList()));
    }

    extension<T, TResult>(Expression<Func<T, TResult>> source)
    {
        /// <remarks>https://www.c-sharpcorner.com/UploadFile/04fe4a/predicate-combinators-in-linq/#listing13</remarks>
        private Expression<Func<TResult>> WithParametersOf(Expression<Func<T, TResult>> right)
        {
            return new ReplaceParameterVisitor<Func<TResult>>(source.Parameters[0], right.Parameters[0]).Visit(source);
        }
    }
}
