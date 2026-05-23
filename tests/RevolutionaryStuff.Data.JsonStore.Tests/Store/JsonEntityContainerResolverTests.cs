using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Tests.Store;

[TestClass]
public class AttributeJsonEntityContainerResolverTests
{
    #region Test entity hierarchy

    [JsonEntityContainerId("root-container")]
    private class RootEntity { }

    private class ChildEntity : RootEntity { }

    private class GrandChildEntity : ChildEntity { }

    [JsonEntityContainerId("override-container")]
    private class OverriddenChildEntity : RootEntity { }

    private class NoAttributeEntity { }

    #endregion

    private static AttributeJsonEntityContainerResolver CreateResolver() => new();

    [TestMethod]
    public void Order_Is2000()
    {
        Assert.AreEqual(2000, CreateResolver().Order);
    }

    [TestMethod]
    public void ResolveContainerId_DirectAttribute_ReturnsContainerId()
    {
        var resolver = CreateResolver();
        Assert.AreEqual("root-container", resolver.ResolveContainerId(typeof(RootEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_InheritedAttribute_ReturnsBaseContainerId()
    {
        var resolver = CreateResolver();
        Assert.AreEqual("root-container", resolver.ResolveContainerId(typeof(ChildEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_GrandChildInheritsAttribute_ReturnsBaseContainerId()
    {
        var resolver = CreateResolver();
        Assert.AreEqual("root-container", resolver.ResolveContainerId(typeof(GrandChildEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_OverriddenAttribute_ReturnsOwnContainerId()
    {
        var resolver = CreateResolver();
        Assert.AreEqual("override-container", resolver.ResolveContainerId(typeof(OverriddenChildEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_NoAttribute_ReturnsNull()
    {
        var resolver = CreateResolver();
        Assert.IsNull(resolver.ResolveContainerId(typeof(NoAttributeEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_CalledTwice_ReturnsSameResult()
    {
        var resolver = CreateResolver();
        var first = resolver.ResolveContainerId(typeof(ChildEntity));
        var second = resolver.ResolveContainerId(typeof(ChildEntity));
        Assert.AreEqual(first, second);
    }
}

[TestClass]
public class ConfigJsonEntityContainerResolverTests
{
    #region Test entity hierarchy

    private class BaseEntity { }

    private class DerivedEntity : BaseEntity { }

    private class UnregisteredEntity { }

    #endregion

    private static ConfigJsonEntityContainerResolver CreateResolver(Dictionary<string, string>? map = null)
    {
        var config = new JsonEntityContainerResolverConfig { ContainerIdByTypeName = map ?? [] };
        return new ConfigJsonEntityContainerResolver(Options.Create(config));
    }

    [TestMethod]
    public void Order_Is1000()
    {
        Assert.AreEqual(1000, CreateResolver().Order);
    }

    [TestMethod]
    public void ResolveContainerId_ByShortName_ReturnsContainerId()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            ["BaseEntity"] = "base-container"
        });
        Assert.AreEqual("base-container", resolver.ResolveContainerId(typeof(BaseEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_ByFullName_ReturnsContainerId()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            [typeof(BaseEntity).FullName!] = "base-container"
        });
        Assert.AreEqual("base-container", resolver.ResolveContainerId(typeof(BaseEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_DerivedTypeMatchesBaseEntry_ReturnsBaseContainerId()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            ["BaseEntity"] = "base-container"
        });
        Assert.AreEqual("base-container", resolver.ResolveContainerId(typeof(DerivedEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_UnregisteredType_ReturnsNull()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            ["BaseEntity"] = "base-container"
        });
        Assert.IsNull(resolver.ResolveContainerId(typeof(UnregisteredEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_EmptyMap_ReturnsNull()
    {
        var resolver = CreateResolver();
        Assert.IsNull(resolver.ResolveContainerId(typeof(BaseEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_DerivedEntryTakesPrecedenceOverBase()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            ["BaseEntity"] = "base-container",
            ["DerivedEntity"] = "derived-container"
        });
        Assert.AreEqual("derived-container", resolver.ResolveContainerId(typeof(DerivedEntity)));
    }

    [TestMethod]
    public void ResolveContainerId_CalledTwice_ReturnsSameResult()
    {
        var resolver = CreateResolver(new Dictionary<string, string>
        {
            ["BaseEntity"] = "base-container"
        });
        var first = resolver.ResolveContainerId(typeof(DerivedEntity));
        var second = resolver.ResolveContainerId(typeof(DerivedEntity));
        Assert.AreEqual(first, second);
    }
}

[TestClass]
public class JsonEntityContainerIdAttributeTests
{
    [JsonEntityContainerId("my-container")]
    private class AnnotatedEntity { }

    private class UnannotatedEntity { }

    [TestMethod]
    public void GetContainerId_WithAttribute_ReturnsContainerId()
    {
        Assert.AreEqual("my-container", JsonEntityContainerIdAttribute.GetContainerId(typeof(AnnotatedEntity)));
    }

    [TestMethod]
    public void GetContainerId_Generic_WithAttribute_ReturnsContainerId()
    {
        Assert.AreEqual("my-container", JsonEntityContainerIdAttribute.GetContainerId<AnnotatedEntity>());
    }

    [TestMethod]
    public void GetContainerId_WithoutAttribute_ThrowsInvalidOperationException()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => JsonEntityContainerIdAttribute.GetContainerId(typeof(UnannotatedEntity)));
        StringAssert.Contains(ex.Message, typeof(UnannotatedEntity).FullName);
    }
}

[TestClass]
public class ResolverOrderingTests
{
    #region Test entity hierarchy

    [JsonEntityContainerId("attribute-container")]
    private class AttributeEntity { }

    #endregion

    [TestMethod]
    public void ConfigResolver_WinsOver_AttributeResolver_WhenBothMatch()
    {
        var configResolver = new ConfigJsonEntityContainerResolver(Options.Create(new JsonEntityContainerResolverConfig
        {
            ContainerIdByTypeName = new Dictionary<string, string> { ["AttributeEntity"] = "config-container" }
        }));
        var attributeResolver = new AttributeJsonEntityContainerResolver();

        var resolvers = new List<IJsonEntityContainerResolver> { attributeResolver, configResolver };
        resolvers.Sort((a, b) => a.Order.CompareTo(b.Order));

        string? result = null;
        foreach (var r in resolvers)
        {
            result = r.ResolveContainerId(typeof(AttributeEntity));
            if (result != null) break;
        }

        Assert.AreEqual("config-container", result);
    }

    [TestMethod]
    public void AttributeResolver_UsedAsFallback_WhenConfigMisses()
    {
        var configResolver = new ConfigJsonEntityContainerResolver(Options.Create(new JsonEntityContainerResolverConfig()));
        var attributeResolver = new AttributeJsonEntityContainerResolver();

        var resolvers = new List<IJsonEntityContainerResolver> { attributeResolver, configResolver };
        resolvers.Sort((a, b) => a.Order.CompareTo(b.Order));

        string? result = null;
        foreach (var r in resolvers)
        {
            result = r.ResolveContainerId(typeof(AttributeEntity));
            if (result != null) break;
        }

        Assert.AreEqual("attribute-container", result);
    }
}
