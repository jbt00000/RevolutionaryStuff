using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Database;

namespace RevolutionaryStuff.Core.Tests.Database;

[TestClass]
public class SqlHelpersTests
{
    [TestMethod]
    public void EscapeForSqlTestEmpty()
    {
        Assert.AreEqual("", "".EscapeForSql());
    }

    [TestMethod]
    public void EscapeForSqlTestHasOneSingleQuote()
    {
        Assert.AreEqual("a''b", "a'b".EscapeForSql());
    }

    [TestMethod]
    public void EscapeForSqlTestHasOneDoubleQuote()
    {
        Assert.AreEqual("a\"b", "a\"b".EscapeForSql());
    }
}
