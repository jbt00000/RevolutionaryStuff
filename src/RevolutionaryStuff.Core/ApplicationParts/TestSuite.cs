using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.ApplicationParts;

public abstract class TestSuite<TSubject> : BaseDisposable
{
    protected IServiceProvider ServiceProvider { get; }
    protected IServiceScope TestScope { get; private set; }
    protected IServiceProvider TestScopeServiceProvider
        => TestScope.ServiceProvider;
    protected TSubject Subject { get; private set; }

    protected TestSuite(IServiceProvider serviceProvider)
    {
        Requires.NonNull(serviceProvider);
        ServiceProvider = serviceProvider;
    }

    protected virtual TSubject GetSubject()
        => TestScopeServiceProvider.GetRequiredService<TSubject>();

    [TestInitialize]
    public void PreTest()
    {
        Assert.IsNull(TestScope);
        TestScope = ServiceProvider.CreateScope();
        Subject = GetSubject();
        OnPreTest();
    }

    protected void OnPreTest()
    { }

    [TestCleanup]
    public virtual void PostTest()
    {
        Assert.IsNotNull(TestScope);
        try
        {
            OnPostTest();
        }
        finally
        {
            Stuff.Dispose(Subject);
            Subject = default;
            TestScope.Dispose();
            TestScope = null;
        }
    }

    protected void OnPostTest()
    { }
}
