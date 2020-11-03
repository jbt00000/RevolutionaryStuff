using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class FileSystemHelperTests
    {
        [TestMethod]
        public void GetParentDirectoryTestUp1Success()
        {
            foreach (var kvp in new Dictionary<string, string> {
                { "c:\\a", "c:\\" },
                { "c:\\a\\", "c:\\" },
                { "c:\\aaaa", "c:\\" },
                { "c:\\aaaa\\", "c:\\" },
                { "c:\\1\\a", "c:\\1" },
                { "c:\\1\\a\\", "c:\\1" },
                { "c:\\1\\aaaa", "c:\\1" },
                { "c:\\1\\aaaa\\", "c:\\1" },
            })
            {
                var actual = FileSystemHelpers.GetParentDirectory(kvp.Key);
                Assert.AreEqual(kvp.Value, actual);
            }        
        }
    }
}
