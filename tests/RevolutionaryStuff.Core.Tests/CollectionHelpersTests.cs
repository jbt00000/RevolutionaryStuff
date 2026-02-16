using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class CollectionHelpersTests
{
    #region Test Helpers

    private static IList<int> CreateInOrderList(int elementCount = 100)
        => Enumerable.Range(0, elementCount).ToList();

    private void ValidateAllElementsExactlyOnce(IList<int> l, int start = 0, int count = 100)
    {
        var d = l.ToDictionary(a => a);
        Assert.HasCount(count, d);
        var orderedKeys = d.Keys.OrderBy().ToList();
        Assert.AreEqual(start, orderedKeys[0]);
    }

    #endregion

    #region Null-Safe Operations Tests

    [TestMethod]
    public void EnumerateNullList()
    {
        IList<int> l = null;
        Assert.Throws<Exception>(() =>
        {
            foreach (var e in l)
            { }
        });
    }

    [TestMethod]
    public void SafeEnumerateNullList()
    {
        IList<int> l = null;
        var cnt = 0;
        foreach (var e in l.NullSafeEnumerable())
        {
            ++cnt;
        }
        Assert.AreEqual(0, cnt);
    }

    [TestMethod]
    public void SafeEnumerateNullDictionary()
    {
        IDictionary<int, object> l = null;
        var cnt = 0;
        foreach (var e in l.NullSafeEnumerable())
        {
            ++cnt;
        }
        Assert.AreEqual(0, cnt);
    }

    [TestMethod]
    public void NullSafeCount_WithNull_ReturnsZero()
    {
        IEnumerable<int> items = null;
        Assert.AreEqual(0, items.NullSafeCount());
    }

    [TestMethod]
    public void NullSafeCount_WithList_ReturnsCount()
    {
        var items = new List<int> { 1, 2, 3 };
        Assert.AreEqual(3, items.NullSafeCount());
    }

    [TestMethod]
    public void NullSafeAny_WithNull_ReturnsFalse()
    {
        IEnumerable<int> items = null;
        Assert.IsFalse(items.NullSafeAny());
    }

    [TestMethod]
    public void NullSafeAny_WithEmpty_ReturnsFalse()
    {
        var items = new List<int>();
        Assert.IsFalse(items.NullSafeAny());
    }

    [TestMethod]
    public void NullSafeAny_WithItems_ReturnsTrue()
    {
        var items = new List<int> { 1 };
        Assert.IsTrue(items.NullSafeAny());
    }

    [TestMethod]
    public void NullSafeAny_WithPredicate_FiltersCorrectly()
    {
        var items = new List<int> { 1, 2, 3, 4 };
        Assert.IsTrue(items.NullSafeAny(x => x > 2));
        Assert.IsFalse(items.NullSafeAny(x => x > 10));
    }

    #endregion

    #region SetIf Tests

    [TestMethod]
    public void SetIfValNotNull_ValNotNullTest()
    {
        var d = new Dictionary<int, object>();
        d.SetIfValNotNull(0, 1);
        Assert.HasCount(1, d);
        d.SetIfValNotNull(1, new object());
        Assert.HasCount(2, d);
        d.SetIfValNotNull(1, "hello");
        Assert.HasCount(2, d);
    }

    [TestMethod]
    public void SetIfValNotNull_ValIsNullTest()
    {
        var d = new Dictionary<int, object>();
        d.SetIfValNotNull(0, null);
        Assert.HasCount(0, d);
    }

    [TestMethod]
    public void SetIfKeyNotFound_KeysAreNotAlreadyThere()
    {
        var d = new Dictionary<int, object>();
        d.SetIfKeyNotFound(1, "a");
        d.SetIfKeyNotFound(2, "b");
        d.SetIfKeyNotFound(3, "c");
        Assert.HasCount(3, d);
    }

    [TestMethod]
    public void SetIfKeyNotFound_KeysAreThere()
    {
        var d = new Dictionary<int, object>();
        d.SetIfKeyNotFound(1, "a");
        d.SetIfKeyNotFound(2, "b");
        d.SetIfKeyNotFound(3, "c");
        Assert.HasCount(3, d);
        d.SetIfKeyNotFound(1, 2);
        Assert.AreEqual("a", d[1]);
    }

    #endregion

    #region Remove Tests

    [TestMethod]
    public void RemoveWhereNone()
    {
        var items = Enumerable.Range(0, 100).ToList();
        items.Remove(z => false);
        Assert.HasCount(100, items);
    }

    [TestMethod]
    public void RemoveWhereAll()
    {
        var items = Enumerable.Range(0, 100).ToList();
        items.Remove(z => true);
        Assert.HasCount(0, items);
    }

    [TestMethod]
    public void RemoveWhereOdd()
    {
        var items = Enumerable.Range(0, 100).ToList();
        items.Remove(z => z.IsOdd());
        Assert.HasCount(50, items);
    }

    [TestMethod]
    public void Remove_MultipleItems_RemovesCorrectly()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var removed = items.Remove(new[] { 2, 4 });
        Assert.AreEqual(2, removed);
        Assert.HasCount(3, items);
    }

    #endregion

    #region Shuffle and Random Tests

    [TestMethod]
    public void ShuffleTest()
    {
        var a = CreateInOrderList();
        var b = CreateInOrderList();
        ValidateAllElementsExactlyOnce(a);
        ValidateAllElementsExactlyOnce(b);
        b.Shuffle();
        ValidateAllElementsExactlyOnce(b);
        for (var z = 0; z < a.Count; ++z)
        {
            if (a[z] != b[z]) return;
        }
        Assert.Fail();
    }

    [TestMethod]
    public void Shuffle_EmptyList_DoesNotThrow()
    {
        var list = new List<int>();
        list.Shuffle();
        Assert.HasCount(0, list);
    }

    [TestMethod]
    public void Shuffle_SingleElement_NoChange()
    {
        var list = new List<int> { 42 };
        list.Shuffle();
        Assert.HasCount(1, list);
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void RandomElement_WithItems_ReturnsItem()
    {
        var list = new List<int> { 1, 2, 3 };
        var element = list.RandomElement();
        Assert.IsTrue(list.Contains(element));
    }

    [TestMethod]
    public void RandomElement_EmptyList_ReturnsDefault()
    {
        var list = new List<int>();
        var element = list.RandomElement();
        Assert.AreEqual(default, element);
    }

    #endregion

    #region Stack and Queue Operations

    [TestMethod]
    public void Dequeue_RemovesFirstElement()
    {
        var list = new List<int> { 1, 2, 3 };
        var first = list.Dequeue();
        Assert.AreEqual(1, first);
        Assert.HasCount(2, list);
        Assert.AreEqual(2, list[0]);
    }

    [TestMethod]
    public void Pop_RemovesLastElement()
    {
        var list = new List<int> { 1, 2, 3 };
        var last = list.Pop();
        Assert.AreEqual(3, last);
        Assert.HasCount(2, list);
    }

    #endregion

    #region ToStringStringKeyValuePairs Tests

    [TestMethod]
    public void ToStringStringKeyValuePairsTest()
    {
        var ins = new List<KeyValuePair<string, object>> {
            new("a", 1),
            new("b", 2),
            new("b", "jason"),
            new("c", null)
        };
        var outs = ins.ToStringStringKeyValuePairs();
        Assert.HasCount(ins.Count, outs);
        for (var z = 0; z < ins.Count; ++z)
        {
            var i = ins[z];
            var o = outs[z];
            Assert.AreEqual(i.Key, o.Key);
            if (i.Value == null)
            {
                Assert.IsNull(o.Value);
            }
            else
            {
                Assert.AreEqual(i.Value.ToString(), o.Value);
            }
        }
    }

    #endregion

    #region OrderBy Tests

    [TestMethod]
    public void OrderBy_Ascending()
    {
        var items = new[] { 3, 1, 4, 1, 5, 9, 2, 6 };
        var result = items.OrderBy(x => x, isAscending: true).ToList();
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(9, result[^1]);
    }

    [TestMethod]
    public void OrderBy_Descending()
    {
        var items = new[] { 3, 1, 4, 1, 5, 9, 2, 6 };
        var result = items.OrderBy(x => x, isAscending: false).ToList();
        Assert.AreEqual(9, result[0]);
        Assert.AreEqual(1, result[^1]);
    }

    [TestMethod]
    public void OrderBy_Comparable_SortsCorrectly()
    {
        var items = new List<int> { 3, 1, 2 };
        var result = items.OrderBy();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToList());
    }

    #endregion

    #region HashSet Tests

    [TestMethod]
    public void AddRange_HashSet_AddsItems()
    {
        var set = new HashSet<int>();
        set.AddRange(new[] { 1, 2, 3 });
        Assert.HasCount(3, set);
    }

    [TestMethod]
    public void AddRange_WithDuplicates_AddsUnique()
    {
        var set = new HashSet<int> { 1 };
        set.AddRange(new[] { 1, 2, 3 });
        Assert.HasCount(3, set);
    }

    [TestMethod]
    public void ContainsAnyElement_WithOverlap_ReturnsTrue()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Assert.IsTrue(set.ContainsAnyElement(new[] { 3, 4, 5 }));
    }

    [TestMethod]
    public void ContainsAnyElement_NoOverlap_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Assert.IsFalse(set.ContainsAnyElement(new[] { 4, 5, 6 }));
    }

    #endregion

    #region Map Tests

    [TestMethod]
    public void Map_BasicMapping()
    {
        var items = new[] { 1, 2, 3 };
        var map = new Dictionary<int, string> { { 1, "one" }, { 2, "two" }, { 3, "three" } };
        var result = items.Map(map, x => x);
        CollectionAssert.AreEqual(new[] { "one", "two", "three" }, result.ToList());
    }

    [TestMethod]
    public void Map_WithMissingKeys_IncludesDefaults()
    {
        var items = new[] { 1, 2, 4 };
        var map = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };
        var result = items.Map(map, x => x, omitMissing: false);
        Assert.HasCount(3, result);
        Assert.IsNull(result[2]);
    }

    [TestMethod]
    public void Map_WithTransform()
    {
        var items = new[] { 1, 2 };
        var map = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };
        var result = items.Map(map, x => x, s => s.ToUpper());
        CollectionAssert.AreEqual(new[] { "ONE", "TWO" }, result.ToList());
    }

    #endregion

    #region ToDictionary Tests

    [TestMethod]
    public void ToDictionaryOnConflictKeepLast_KeepsLastValue()
    {
        var items = new[] {
            new KeyValuePair<int, string>(1, "first"),
            new KeyValuePair<int, string>(1, "second")
        };
        var dict = items.ToDictionaryOnConflictKeepLast(x => x.Key, x => x.Value);
        Assert.AreEqual("second", dict[1]);
    }

    #endregion

    #region ToMultipleValueDictionary Tests

    [TestMethod]
    public void ToMultipleValueDictionary_GroupsCorrectly()
    {
        var items = new[] { 1, 2, 3, 4, 5, 6 };
        var mvd = items.ToMultipleValueDictionary(x => x % 2);
        Assert.HasCount(2, mvd);
        Assert.HasCount(3, mvd[0]); // Even numbers
        Assert.HasCount(3, mvd[1]); // Odd numbers
    }

    #endregion

    #region FirstValueOfType Tests

    [TestMethod]
    public void FirstValueOfType_FindsCorrectType()
    {
        var dict = new Dictionary<string, object>
        {
            { "a", 1 },
            { "b", "hello" },
            { "c", 3.14 }
        };
        Assert.AreEqual("hello", dict.FirstValueOfType<string>());
        Assert.AreEqual(1, dict.FirstValueOfType<int>());
    }

    [TestMethod]
    public void FirstValueOfType_NotFound_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "a", 1 } };
        Assert.IsNull(dict.FirstValueOfType<string>());
    }

    #endregion

    #region Collection Helpers

    [TestMethod]
    public void AsReadOnly_CreatesReadOnlyList()
    {
        var items = new[] { 1, 2, 3 };
        var readOnly = items.AsReadOnly();
        Assert.HasCount(3, readOnly);
        Assert.Throws<NotSupportedException>(() => readOnly.Add(4));
    }

    [TestMethod]
    public void WhereNotNull_FiltersNulls()
    {
        var items = new[] { "a", null, "b", null, "c" };
        var result = items.WhereNotNull().ToList();
        Assert.HasCount(3, result);
    }

    #endregion

    #region Format Tests

    [TestMethod]
    public void Format_WithSeparator()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.Format(", ");
        Assert.AreEqual("1, 2, 3", result);
    }

    [TestMethod]
    public void Format_WithFormatter()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.Format(", ", (item, index) => $"[{index}]={item}");
        Assert.AreEqual("[0]=1, [1]=2, [2]=3", result);
    }

    [TestMethod]
    public void Format_Null_ReturnsEmpty()
    {
        IEnumerable<int> items = null;
        Assert.AreEqual("", items.Format(","));
    }

    #endregion

    #region GetValue and Find Tests

    [TestMethod]
    public void GetValue_ExistingKey_ReturnsValue()
    {
        var dict = new Dictionary<int, string> { { 1, "one" } };
        Assert.AreEqual("one", dict.GetValue(1));
    }

    [TestMethod]
    public void GetValue_MissingKey_ReturnsFallback()
    {
        var dict = new Dictionary<int, string>();
        Assert.AreEqual("default", dict.GetValue(1, "default"));
    }

    [TestMethod]
    public void FindOrCreate_ExistingKey_ReturnsExisting()
    {
        var dict = new Dictionary<int, string> { { 1, "one" } };
        var result = dict.FindOrCreate(1, () => "new");
        Assert.AreEqual("one", result);
    }

    [TestMethod]
    public void FindOrCreate_MissingKey_CreatesNew()
    {
        var dict = new Dictionary<int, string>();
        var result = dict.FindOrCreate(1, () => "new");
        Assert.AreEqual("new", result);
        Assert.HasCount(1, dict);
    }

    #endregion

    #region ForEach Tests

    [TestMethod]
    public void ForEach_ExecutesAction()
    {
        var items = new[] { 1, 2, 3 };
        var sum = 0;
        items.ForEach(x => sum += x);
        Assert.AreEqual(6, sum);
    }

    [TestMethod]
    public void ForEach_WithIndex_ProvidesIndex()
    {
        var items = new[] { "a", "b", "c" };
        var result = new List<string>();
        items.ForEach((item, index) => result.Add($"{index}:{item}"));
        Assert.AreEqual("0:a", result[0]);
        Assert.AreEqual("2:c", result[2]);
    }

    #endregion

    #region ToSet Tests

    [TestMethod]
    public void ToSet_CreatesHashSet()
    {
        var items = new[] { 1, 2, 2, 3 };
        var set = items.ToSet();
        Assert.HasCount(3, set);
    }

    [TestMethod]
    public void ToSet_WithConverter_TransformsItems()
    {
        var items = new[] { 1, 2, 3 };
        var set = items.ToSet(x => x * 2);
        CollectionAssert.AreEquivalent(new[] { 2, 4, 6 }, set.ToList());
    }

    [TestMethod]
    public void ToCaseInsensitiveSet_IsCaseInsensitive()
    {
        var items = new[] { "Hello", "hello", "HELLO" };
        var set = items.ToCaseInsensitiveSet();
        Assert.HasCount(1, set);
    }

    #endregion

    #region Increment Tests

    [TestMethod]
    public void Increment_NewKey_InitializesToOne()
    {
        var dict = new Dictionary<int, int>();
        var result = dict.Increment(1);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Increment_ExistingKey_Increments()
    {
        var dict = new Dictionary<int, int> { { 1, 5 } };
        var result = dict.Increment(1);
        Assert.AreEqual(6, result);
    }

    [TestMethod]
    public void Increment_WithAmount()
    {
        var dict = new Dictionary<int, int>();
        dict.Increment(1, 10);
        Assert.AreEqual(10, dict[1]);
        dict.Increment(1, 5);
        Assert.AreEqual(15, dict[1]);
    }

    #endregion

    #region Chunkify Tests

    [TestMethod]
    public void Chunkify_SplitsIntoChunks()
    {
        var items = Enumerable.Range(1, 10);
        var chunks = items.Chunkify(3);
        Assert.HasCount(4, chunks);
        Assert.HasCount(3, chunks[0]);
        Assert.HasCount(3, chunks[1]);
        Assert.HasCount(3, chunks[2]);
        Assert.HasCount(1, chunks[3]);
    }

    #endregion

    #region IndexOfOccurrence Tests

    [TestMethod]
    public void IndexOfOccurrence_FindsFirstOccurrence()
    {
        var items = new[] { 1, 2, 3, 2, 4 };
        var index = items.IndexOfOccurrence(2, 1);
        Assert.AreEqual(1, index);
    }

    [TestMethod]
    public void IndexOfOccurrence_FindsSecondOccurrence()
    {
        var items = new[] { 1, 2, 3, 2, 4 };
        var index = items.IndexOfOccurrence(2, 2);
        Assert.AreEqual(3, index);
    }

    [TestMethod]
    public void IndexOfOccurrence_NotFound_ReturnsMissing()
    {
        var items = new[] { 1, 2, 3 };
        var index = items.IndexOfOccurrence(5, 1, missingValue: -1);
        Assert.AreEqual(-1, index);
    }

    [TestMethod]
    public void IndexOfOccurrence_WithPredicate()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        var index = items.IndexOfOccurrence(x => x > 3, 2);
        Assert.AreEqual(4, index);
    }

    #endregion

    #region Fluent Tests

    [TestMethod]
    public void FluentAdd_AddsAndReturns()
    {
        var list = new List<int>().FluentAdd(1).FluentAdd(2);
        Assert.HasCount(2, list);
    }

    [TestMethod]
    public void FluentAddRange_AddsMultiple()
    {
        var list = new List<int>().FluentAddRange(new[] { 1, 2, 3 });
        Assert.HasCount(3, list);
    }

    [TestMethod]
    public void FluentClear_ClearsAndReturns()
    {
        var list = new List<int> { 1, 2, 3 }.FluentClear();
        Assert.HasCount(0, list);
    }

    #endregion

    #region Async Tests

    [TestMethod]
    public async Task ToListAsync_ConvertsAsyncEnumerable()
    {
        async IAsyncEnumerable<int> GetItemsAsync()
        {
            await Task.Delay(1);
            yield return 1;
            yield return 2;
            yield return 3;
        }

        var result = await GetItemsAsync().ToListAsync();
        Assert.HasCount(3, result);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToList());
    }

    #endregion

    #region TryGetValueIgnoreCase Tests

    [TestMethod]
    public void TryGetValueIgnoreCase_ExactMatch()
    {
        var dict = new Dictionary<string, int> { { "Hello", 1 } };
        Assert.IsTrue(dict.TryGetValueIgnoreCase("Hello", out var value));
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TryGetValueIgnoreCase_CaseInsensitive()
    {
        var dict = new Dictionary<string, int> { { "Hello", 1 } };
        Assert.IsTrue(dict.TryGetValueIgnoreCase("HELLO", out var value));
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TryGetValueIgnoreCase_NotFound()
    {
        var dict = new Dictionary<string, int> { { "Hello", 1 } };
        Assert.IsFalse(dict.TryGetValueIgnoreCase("World", out _));
    }

    #endregion

    #region AddFormat Tests

    [TestMethod]
    public void AddFormat_AddsFormattedString()
    {
        var list = new List<string>();
        list.AddFormat("Hello {0}", "World");
        Assert.AreEqual("Hello World", list[0]);
    }

    #endregion

    #region ToOrderedValuesList Tests

    [TestMethod]
    public void ToOrderedValuesList_ReturnsOrderedValues()
    {
        var dict = new Dictionary<int, string>
        {
            { 1, "one" }, { 2, "two" }, { 3, "three" }
        };
        var result = dict.ToOrderedValuesList(new[] { 3, 1, 2 });
        CollectionAssert.AreEqual(new[] { "three", "one", "two" }, result);
    }

    [TestMethod]
    public void ToOrderedValuesList_MissingKey_UsesMissingVal()
    {
        var dict = new Dictionary<int, string> { { 1, "one" } };
        var result = dict.ToOrderedValuesList(new[] { 1, 2 }, throwOnMiss: false, missingVal: "missing");
        Assert.AreEqual("missing", result[1]);
    }

    #endregion

    #region HasData Tests

    [TestMethod]
    public void HasData_WithItems_ReturnsTrue()
    {
        var items = new[] { 1, 2, 3 };
        Assert.IsTrue(items.HasData());
    }

    [TestMethod]
    public void HasData_Empty_ReturnsFalse()
    {
        var items = Array.Empty<int>();
        Assert.IsFalse(items.HasData());
    }

    [TestMethod]
    public void HasData_Null_ReturnsFalse()
    {
        IEnumerable<int> items = null;
        Assert.IsFalse(items.HasData());
    }

    #endregion
}
