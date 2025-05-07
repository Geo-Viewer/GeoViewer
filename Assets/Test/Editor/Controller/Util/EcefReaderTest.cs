using System;
using System.IO;
using GeoViewer.Controller.Util;
using NUnit.Framework;
using Unity.Mathematics;

namespace GeoViewer.Test.Editor.Controller.Util
{
    public class EcefReaderTest
    {
        private const string TestFile1 = @"# OFFSET_X OFFSET_Y OFFSET_Z
5015869.524 -280006.784 3917285.176";

        private const string TestFile2 = @"# OFFSET_X OFFSET_Y OFFSET_Z
4147204.614 606567.079 4791650.573";

        [Test]
        public void BridgeReadTest()
        {
            var result = EcefReader.ReadEcefFromFile(StringToStream(TestFile1));
            Assert.AreEqual(new double3(5015869.524, -280006.784, 3917285.176), result);
        }

        [Test]
        public void ThermoselectReadTest()
        {
            var result = EcefReader.ReadEcefFromFile(StringToStream(TestFile2));
            Assert.AreEqual(new double3(4147204.614, 606567.079, 4791650.573), result);
        }

        [Test]
        public void EmptyReadTest()
        {
            Assert.Throws(typeof(FormatException), () => EcefReader.ReadEcefFromFile(StringToStream("42")));
        }

        private Stream StringToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}