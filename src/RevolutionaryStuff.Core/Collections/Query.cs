using System.Collections;
using System.Linq.Expressions;

namespace RevolutionaryStuff.Core.Collections;

public class Query<TSource> : IOrderedQueryable<TSource>
{
    public Query(IQueryProvider provider, IQueryable<TSource> innerSource)
    {
        Provider = provider;
        Expression = Expression.Constant(innerSource);
    }

    public Query(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    #region IEnumerable<TSource> Members

    public IEnumerator<TSource> GetEnumerator()
    {
        return Provider.Execute<IEnumerable<TSource>>(Expression).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region IQueryable Members

    public Type ElementType
    {
        get
        {
            return typeof(TSource);
        }
    }

    public Expression Expression
    {
        get;
        private set;
    }

    public IQueryProvider Provider
    {
        get;
        private set;
    }

    #endregion
}
