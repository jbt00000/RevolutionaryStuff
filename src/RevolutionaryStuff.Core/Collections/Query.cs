using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RevolutionaryStuff.Core.Collections
{
    public class Query<TSource> : IOrderedQueryable<TSource>
    {
        public Query(IQueryProvider provider, IQueryable<TSource> innerSource)
        {
            this.Provider = provider;
            this.Expression = Expression.Constant(innerSource);
        }

        public Query(IQueryProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        #region IEnumerable<TSource> Members

        public IEnumerator<TSource> GetEnumerator()
        {
            return this.Provider.Execute<IEnumerable<TSource>>(this.Expression).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
}
