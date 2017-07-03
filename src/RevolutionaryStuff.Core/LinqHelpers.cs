using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RevolutionaryStuff.Core
{
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
        }

        public static IList<MemberInfo> GetMembers(this LambdaExpression e)
        {
            var memberInfos = new List<MemberInfo>();

            MemberExpression body = e.Body as MemberExpression;
            Again:
            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)e.Body;
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

            MemberExpression body = exp;
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

        public static string GetFullyQualifiedName<TModel, TResult>(this Expression<Func<TModel, TResult>> exp)
        {
            return exp.GetMembers().ConvertAll(z => z.Name).Format(".");
        }
    }
}
