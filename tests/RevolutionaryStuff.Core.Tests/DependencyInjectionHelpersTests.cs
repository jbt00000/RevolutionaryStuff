using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class DependencyInjectionHelpersTests
{
    public interface IDep
    {
        int TheAnswer { get; }
    }

    public class Dep5 : IDep
    {
        public const int FixedAnswer = 5;
        int IDep.TheAnswer => FixedAnswer;
    }

    public class DepHardcoded : IDep
    {
        public DepHardcoded(int theAnswer)
        {
            TheAnswer = theAnswer;
        }

        public int TheAnswer { get; }
    }

    public class ClassA
    {
        private readonly IDep Dep;

        public ClassA(IDep dep)
        {
            Dep = dep;
        }

        public int Echo => Dep.TheAnswer;
    }

    [TestMethod]
    public void InstantiateServiceWithOverriddenDependenciesTest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDep, Dep5>();
        services.AddScoped<ClassA>();
        var serviceProvider = services.BuildServiceProvider();
        int aHashCodeScope1;
        using (var scope = serviceProvider.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var a = sp.GetRequiredService<ClassA>();
            Assert.AreEqual(Dep5.FixedAnswer, a.Echo);
            aHashCodeScope1 = a.GetHashCode();
        }
        using (var scope = serviceProvider.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var d = sp.GetRequiredService<IDep>();
            Assert.AreEqual(Dep5.FixedAnswer, d.TheAnswer);
            var b = sp.GetRequiredService<ClassA>();
            Assert.AreEqual(Dep5.FixedAnswer, b.Echo);
            Assert.AreNotEqual(aHashCodeScope1, b.GetHashCode());
            var c = sp.GetRequiredService<ClassA>();
            Assert.AreEqual(b.GetHashCode(), c.GetHashCode());
            Assert.AreSame(b, c);
        }
        using (var scope = serviceProvider.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var d = new DepHardcoded(42);
            var a = sp.GetRequiredService<ClassA>();
            Assert.AreEqual(Dep5.FixedAnswer, a.Echo);
        }
    }
}
