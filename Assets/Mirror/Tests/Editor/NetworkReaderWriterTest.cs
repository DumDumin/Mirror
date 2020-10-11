using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkReaderWriterTest
    {
        [Test]
        public void TestIntWriterNotNull()
        {
            Assert.That(Writer<int>.write, Is.Not.Null);
        }

        [Test]
        public void TestIntReaderNotNull()
        {
            Assert.That(Reader<int>.read, Is.Not.Null);
        }

        [Test]
        public void TestAccessingCustomWriterAndReader()
        {
            NetworkWriter writer = new NetworkWriter(1024);
            writer.Write<int>(3);
            NetworkReader reader = new NetworkReader(writer.ToArraySegment());
            int copy = reader.Read<int>();

            Assert.That(copy, Is.EqualTo(3));
        }
    }
}
