﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Database;

namespace RevolutionaryStuff.Core.Tests.Database
{
    [TestClass]
    public class SqlHelpersTests
    {
        [TestMethod]
        public void EscapeForSqlTestEmpty()
        {
            Assert.AreEqual("", SqlHelpers.EscapeForSql(""));
        }

        [TestMethod]
        public void EscapeForSqlTestHasOneSingleQuote()
        {
            Assert.AreEqual("a''b", SqlHelpers.EscapeForSql("a'b"));
        }

        [TestMethod]
        public void EscapeForSqlTestHasOneDoubleQuote()
        {
            Assert.AreEqual("a\"b", SqlHelpers.EscapeForSql("a\"b"));
        }
    }
}
