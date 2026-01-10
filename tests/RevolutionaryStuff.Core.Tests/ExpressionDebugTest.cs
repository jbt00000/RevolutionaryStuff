using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class ExpressionDebugTest
{
    public class TestItem
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void ManualExpression_Simple()
    {
        var items = new List<TestItem> { new() { Value = 1 } }.AsQueryable();

        // Manually build: items.OrderBy(x => x.Value)
        var param = Expression.Parameter(typeof(TestItem), "x");
        var prop = Expression.Property(param, "Value");
        var lambda = Expression.Lambda<Func<TestItem, int>>(prop, param);

        var methodCall = Expression.Call(
            typeof(Queryable),
            "OrderBy",
            new[] { typeof(TestItem), typeof(int) },
            items.Expression,
            Expression.Quote(lambda)
        );

        var result = items.Provider.CreateQuery<TestItem>(methodCall).ToList();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ManualExpression_WithConstant()
    {
        var items = new List<TestItem> { new() { Value = 1 } }.AsQueryable();

        // Manually build: items.OrderBy(x => 100)  // constant
        var param = Expression.Parameter(typeof(TestItem), "x");
        var constant = Expression.Constant(100);
        var lambda = Expression.Lambda<Func<TestItem, int>>(constant, param);

        var methodCall = Expression.Call(
            typeof(Queryable),
            "OrderBy",
            new[] { typeof(TestItem), typeof(int) },
            items.Expression,
            Expression.Quote(lambda)
        );

        var result = items.Provider.CreateQuery<TestItem>(methodCall).ToList();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ManualExpression_WithConditional()
    {
        var items = new List<TestItem> { new() { Value = 1 } }.AsQueryable();

        // Manually build: items.OrderBy(x => x.Value == 1 ? 100 : 200)
        var param = Expression.Parameter(typeof(TestItem), "x");
        var prop = Expression.Property(param, "Value");
        var condition = Expression.Equal(prop, Expression.Constant(1));
        var conditional = Expression.Condition(condition, Expression.Constant(100), Expression.Constant(200));
        var lambda = Expression.Lambda<Func<TestItem, int>>(conditional, param);

        var methodCall = Expression.Call(
            typeof(Queryable),
            "OrderBy",
            new[] { typeof(TestItem), typeof(int) },
            items.Expression,
            Expression.Quote(lambda)
        );

        var result = items.Provider.CreateQuery<TestItem>(methodCall).ToList();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ManualExpression_ExactlyLikeOrderByField()
    {
        var items = new List<TestItem> { new() { Value = 1 } }.AsQueryable();

        // Exactly mimic OrderByField logic
        var map = new Dictionary<int, int> { { 1, 100 } };

        var param = Expression.Parameter(typeof(TestItem), "p");
        var prop = Expression.Property(param, "Value");

        var kvps = map.ToList();
        var expr = (Expression)Expression.Constant(kvps.Last().Value);
        for (var z = kvps.Count - 2; z >= 0; --z)
        {
            var kvp = kvps[z];
            expr = Expression.Condition(
                Expression.Equal(prop, Expression.Constant(kvp.Key)),
                Expression.Constant(kvp.Value),
                expr);
        }

        var lambda = Expression.Lambda<Func<TestItem, int>>(expr, param);
        var methodCall = Expression.Call(
            typeof(Queryable),
            "OrderBy",
            new[] { typeof(TestItem), typeof(int) },
            items.Expression,
            Expression.Quote(lambda)
        );

        var result = items.Provider.CreateQuery<TestItem>(methodCall).ToList();
        Assert.AreEqual(1, result.Count);
    }
}
