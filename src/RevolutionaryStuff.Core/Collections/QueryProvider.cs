using System.Linq.Expressions;
using System.Reflection;

namespace RevolutionaryStuff.Core.Collections;

public abstract class QueryProvider : IQueryProvider
{

    protected QueryProvider()
    { }

    IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression) => new Query<S>(this, expression);


    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        var elementType = TypeSystem.GetElementType(expression.Type);

        try
        {
            return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException;
        }
    }

    S IQueryProvider.Execute<S>(Expression expression) => (S)Execute(expression);

    object IQueryProvider.Execute(Expression expression) => Execute(expression);

    public abstract string GetQueryText(Expression expression);

    public abstract object Execute(Expression expression);

    private static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {

            var ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;

            return ienum.GetGenericArguments()[0];

        }

        private static Type FindIEnumerable(Type seqType)
        {

            if (seqType == null || seqType == typeof(string))

                return null;

            if (seqType.IsArray)

                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.GetTypeInfo().IsGenericType)
            {

                foreach (var arg in seqType.GetGenericArguments())
                {

                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(seqType))
                    {

                        return ienum;

                    }

                }

            }

            var ifaces = seqType.GetInterfaces();

            if (ifaces != null && ifaces.Length > 0)
            {

                foreach (var iface in ifaces)
                {

                    var ienum = FindIEnumerable(iface);

                    if (ienum != null) return ienum;

                }

            }

            if (seqType.GetTypeInfo().BaseType != null && seqType.GetTypeInfo().BaseType != typeof(object))
            {

                return FindIEnumerable(seqType.GetTypeInfo().BaseType);

            }

            return null;

        }

    }
}
