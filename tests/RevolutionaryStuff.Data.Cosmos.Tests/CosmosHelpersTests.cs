using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Data.Cosmos.Tests;

[TestClass]
public class CosmosHelpersTests
{
    private sealed class AggregateItem
    {
        public int Id { get; init; }
        public int IntValue { get; init; }
        public int? NullableIntValue { get; init; }
        public long LongValue { get; init; }
        public long? NullableLongValue { get; init; }
        public float FloatValue { get; init; }
        public float? NullableFloatValue { get; init; }
        public double DoubleValue { get; init; }
        public double? NullableDoubleValue { get; init; }
        public decimal DecimalValue { get; init; }
        public decimal? NullableDecimalValue { get; init; }
    }

    private static readonly IQueryable<AggregateItem> AggregateItems = new[]
    {
        new AggregateItem { Id = 1, IntValue = 1, NullableIntValue = 1, LongValue = 2, NullableLongValue = 2, FloatValue = 3, NullableFloatValue = 3, DoubleValue = 4, NullableDoubleValue = 4, DecimalValue = 5, NullableDecimalValue = 5 },
        new AggregateItem { Id = 2, IntValue = 3, NullableIntValue = null, LongValue = 4, NullableLongValue = null, FloatValue = 5, NullableFloatValue = null, DoubleValue = 6, NullableDoubleValue = null, DecimalValue = 7, NullableDecimalValue = null },
    }.AsQueryable();

    [TestMethod]
    public async Task GetCountAsync_WithInMemoryQuery_ReturnsCount()
    {
        var query = new[] { 1, 2, 3 }.AsQueryable();

        var count = await query.GetCountAsync();

        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public async Task GetCountAsync_WithEmptyInMemoryQuery_ReturnsZero()
    {
        var query = Array.Empty<int>().AsQueryable();

        var count = await query.GetCountAsync();

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task GetCountAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        IQueryable<int> query = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => query.GetCountAsync());
    }

    [TestMethod]
    public async Task GetCountAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        var query = new[] { 1, 2, 3 }.AsQueryable();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => query.GetCountAsync(cancellationTokenSource.Token));
    }

    [TestMethod]
    public async Task GetSumAsync_WithNumericAndNullableSelectors_ReturnsExpectedValues()
    {
        Assert.AreEqual(4, await AggregateItems.GetSumAsync(x => x.IntValue));
        Assert.AreEqual(1, await AggregateItems.GetSumAsync(x => x.NullableIntValue));
        Assert.AreEqual(6L, await AggregateItems.GetSumAsync(x => x.LongValue));
        Assert.AreEqual(2L, await AggregateItems.GetSumAsync(x => x.NullableLongValue));
        Assert.AreEqual(8F, await AggregateItems.GetSumAsync(x => x.FloatValue));
        Assert.AreEqual(3F, await AggregateItems.GetSumAsync(x => x.NullableFloatValue));
        Assert.AreEqual(10D, await AggregateItems.GetSumAsync(x => x.DoubleValue));
        Assert.AreEqual(4D, await AggregateItems.GetSumAsync(x => x.NullableDoubleValue));
        Assert.AreEqual(12M, await AggregateItems.GetSumAsync(x => x.DecimalValue));
        Assert.AreEqual(5M, await AggregateItems.GetSumAsync(x => x.NullableDecimalValue));
    }

    [TestMethod]
    public async Task GetAverageAsync_WithNumericAndNullableSelectors_ReturnsExpectedValues()
    {
        Assert.AreEqual(2D, await AggregateItems.GetAverageAsync(x => x.IntValue));
        Assert.AreEqual(1D, await AggregateItems.GetAverageAsync(x => x.NullableIntValue));
        Assert.AreEqual(3D, await AggregateItems.GetAverageAsync(x => x.LongValue));
        Assert.AreEqual(2D, await AggregateItems.GetAverageAsync(x => x.NullableLongValue));
        Assert.AreEqual(4F, await AggregateItems.GetAverageAsync(x => x.FloatValue));
        Assert.AreEqual(3F, await AggregateItems.GetAverageAsync(x => x.NullableFloatValue));
        Assert.AreEqual(5D, await AggregateItems.GetAverageAsync(x => x.DoubleValue));
        Assert.AreEqual(4D, await AggregateItems.GetAverageAsync(x => x.NullableDoubleValue));
        Assert.AreEqual(6M, await AggregateItems.GetAverageAsync(x => x.DecimalValue));
        Assert.AreEqual(5M, await AggregateItems.GetAverageAsync(x => x.NullableDecimalValue));
    }

    [TestMethod]
    public async Task GetMinAndMaxAsync_WithFilteredNullableQuery_ReturnExpectedValues()
    {
        var query = AggregateItems.Where(x => x.Id > 1);

        Assert.AreEqual(3, await query.GetMinAsync(x => x.IntValue));
        Assert.AreEqual(3, await query.GetMaxAsync(x => x.IntValue));
        Assert.IsNull(await query.GetMinAsync(x => x.NullableIntValue));
        Assert.IsNull(await query.GetMaxAsync(x => x.NullableIntValue));
    }

    [TestMethod]
    public async Task AggregateHelpers_WithEmptyQuery_MatchLinqSemantics()
    {
        var query = Array.Empty<AggregateItem>().AsQueryable();

        Assert.AreEqual(0, await query.GetSumAsync(x => x.IntValue));
        Assert.IsNull(await query.GetAverageAsync(x => x.NullableIntValue));
        Assert.IsNull(await query.GetMinAsync(x => x.NullableIntValue));
        Assert.IsNull(await query.GetMaxAsync(x => x.NullableIntValue));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => query.GetAverageAsync(x => x.IntValue));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => query.GetMinAsync(x => x.IntValue));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => query.GetMaxAsync(x => x.IntValue));
    }

    [TestMethod]
    public async Task AggregateHelpers_WithNullArguments_ThrowArgumentNullException()
    {
        IQueryable<AggregateItem> query = null!;
        Expression<Func<AggregateItem, int>> selector = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => query.GetSumAsync(x => x.IntValue));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => AggregateItems.GetAverageAsync(selector));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => query.GetMinAsync(x => x.IntValue));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => AggregateItems.GetMaxAsync(selector));
    }

    [TestMethod]
    public async Task AggregateHelpers_WithCanceledToken_ThrowOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => AggregateItems.GetSumAsync(x => x.IntValue, cancellationTokenSource.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => AggregateItems.GetAverageAsync(x => x.IntValue, cancellationTokenSource.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => AggregateItems.GetMinAsync(x => x.IntValue, cancellationTokenSource.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => AggregateItems.GetMaxAsync(x => x.IntValue, cancellationTokenSource.Token));
    }
}
