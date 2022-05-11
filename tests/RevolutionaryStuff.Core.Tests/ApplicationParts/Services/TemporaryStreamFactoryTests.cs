using System;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.ApplicationParts.Services;

namespace RevolutionaryStuff.Core.Tests.ApplicationParts.Services
{
    [TestClass]
    public class TemporaryStreamFactoryTests
    {
        private const int MemStreamCapacity = 16 * 1024;

        private static readonly ITemporaryStreamFactory Factory = new TemporaryStreamFactory(new OptionsWrapper<TemporaryStreamFactory.Config>(new TemporaryStreamFactory.Config
        {
            MemoryStreamExpectedCapacityLimit = MemStreamCapacity,
            FileBufferSize = 1024
        }));

        [TestMethod]
        public void WithMemoryStream()
            => CreatedStreamFlexible(MemStreamCapacity / 2, typeof(MemoryStream));

        [TestMethod]
        public void WithFileStream()
            => CreatedStreamFlexible(MemStreamCapacity * 2, typeof(FileStream));

        private void CreatedStreamFlexible(int capacity, Type expectedStreamType)
        {
            using var st = Factory.Create(capacity);
            Assert.AreEqual(expectedStreamType, st.GetType());

            Requires.WriteableStreamArg(st);

            Assert.AreEqual(0, st.Length);
            var buf = new byte[capacity / 2];
            Stuff.RandomWithFixedSeed.NextBytes(buf);
            st.Write(buf);
            Assert.AreEqual(buf.Length, st.Length);
            Assert.AreEqual(buf.Length, st.Position);
            st.Position = 0;

            for (var z = 0; z < 10; ++z)
            {
                Stuff.RandomWithFixedSeed.NextBytes(buf);
                st.Write(buf);
            }
            Assert.AreEqual(10 * buf.Length, st.Length);
            Assert.AreEqual(10 * buf.Length, st.Position);

            const int fixedLen = 10;
            st.SetLength(fixedLen);

            Assert.AreEqual(fixedLen, st.Length);
            Assert.AreEqual(fixedLen, st.Position);
            st.Write(buf);
            Assert.AreEqual(fixedLen + buf.Length, st.Length);
            Assert.AreEqual(fixedLen + buf.Length, st.Position);
        }
    }
}
