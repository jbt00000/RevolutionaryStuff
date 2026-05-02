using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class DisposableBaseTests
{
    #region Test helpers

    private sealed class SimpleDisposable : DisposableBase
    {
        public bool OnDisposeCalled { get; private set; }
        public bool OnDisposeCalledWithDisposing { get; private set; }

        protected override void OnDispose(bool disposing)
        {
            OnDisposeCalled = true;
            OnDisposeCalledWithDisposing = disposing;
        }

        public void PublicCheckNotDisposed() => CheckNotDisposed();

        public bool PublicIsDisposed => IsDisposed;

        public void PublicRegisterDisposeAction(Action a) => RegisterDisposeAction(a);

        public void PublicRegisterDisposableObject(IDisposable d) => RegisterDisposableObject(d);
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public bool WasDisposed { get; private set; }
        public void Dispose() => WasDisposed = true;
    }

    #endregion

    #region IsDisposed

    [TestMethod]
    public void IsDisposed_BeforeDispose_IsFalse()
    {
        var sut = new SimpleDisposable();
        Assert.IsFalse(sut.PublicIsDisposed);
    }

    [TestMethod]
    public void IsDisposed_AfterDispose_IsTrue()
    {
        var sut = new SimpleDisposable();
        sut.Dispose();
        Assert.IsTrue(sut.PublicIsDisposed);
    }

    #endregion

    #region Dispose idempotency

    [TestMethod]
    public void Dispose_CalledMultipleTimes_OnDisposeCalledOnlyOnce()
    {
        var callCount = 0;
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposeAction(() => callCount++);

        sut.Dispose();
        sut.Dispose();
        sut.Dispose();

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Dispose_InvokesOnDisposeWithDisposingTrue()
    {
        var sut = new SimpleDisposable();
        sut.Dispose();

        Assert.IsTrue(sut.OnDisposeCalled);
        Assert.IsTrue(sut.OnDisposeCalledWithDisposing);
    }

    #endregion

    #region CheckNotDisposed

    [TestMethod]
    public void CheckNotDisposed_BeforeDispose_DoesNotThrow()
    {
        var sut = new SimpleDisposable();
        sut.PublicCheckNotDisposed(); // should not throw
    }

    [TestMethod]
    public void CheckNotDisposed_AfterDispose_ThrowsObjectDisposedException()
    {
        var sut = new SimpleDisposable();
        sut.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => sut.PublicCheckNotDisposed());
    }

    #endregion

    #region RegisterDisposeAction

    [TestMethod]
    public void RegisterDisposeAction_IsCalledOnDispose()
    {
        var called = false;
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposeAction(() => called = true);

        sut.Dispose();

        Assert.IsTrue(called);
    }

    [TestMethod]
    public void RegisterDisposeAction_MultipleActions_AllCalledOnDispose()
    {
        var callCount = 0;
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposeAction(() => callCount++);
        sut.PublicRegisterDisposeAction(() => callCount++);
        sut.PublicRegisterDisposeAction(() => callCount++);

        sut.Dispose();

        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public void RegisterDisposeAction_NullAction_IsIgnored()
    {
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposeAction(null); // must not throw
        sut.Dispose();
    }

    #endregion

    #region RegisterDisposableObject

    [TestMethod]
    public void RegisterDisposableObject_DisposesRegisteredObjectOnDispose()
    {
        var tracked = new TrackingDisposable();
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposableObject(tracked);

        sut.Dispose();

        Assert.IsTrue(tracked.WasDisposed);
    }

    [TestMethod]
    public void RegisterDisposableObject_NullObject_IsIgnored()
    {
        var sut = new SimpleDisposable();
        sut.PublicRegisterDisposableObject(null); // must not throw
        sut.Dispose();
    }

    #endregion
}
