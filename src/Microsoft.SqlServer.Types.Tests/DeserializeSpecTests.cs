using System;
using System.IO;
using Xunit;

namespace src
{
    /// <summary>
    /// Deserialize tests based on examples in the UDT specification
    /// </summary>
    public class DeserializeSpecTests
    {
        [Fact]
        public void TestEmptyPoint()
        {
            var d = double.NaN;
            var bits = BitConverter.DoubleToInt64Bits(d);
            bool isFinite = (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
            var emptyPoint = CreateBytes(0, (byte)0x01, (byte)0x04, 0, 0, 1, -1, -1, (byte)0x01);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(emptyPoint));
            Assert.False(g.IsNull);
            Assert.Equal("Point" , g.STGeometryType().Value);
            Assert.Equal(0, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.True(g.M.IsNull);
            Assert.True(g.Z.IsNull);
            Assert.Equal(0, g.STNumGeometries().Value);
        }

        [Fact]
        public void TestPoint()
        {
            var point = CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(point));
            Assert.False(g.IsNull);
            Assert.Equal("Point", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.Equal(5, g.STX.Value);
            Assert.Equal(10, g.STY.Value);
            Assert.False(g.HasZ);
            Assert.False(g.HasM);
            Assert.Equal(1, g.STNumGeometries().Value);
        }

        [Fact]
        public void TestLineString()
        {
            var line = CreateBytes(4326, (byte)0x01, (byte)0x05,
                3, 0d, 1d, 3d, 2d, 4d, 5d, 1d, 2d, double.NaN, //vertices
                1, (byte)0x01, 0, //figures
                1, -1, 0, (byte)0x02 //shapes
                );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(line));
            Assert.False(g.IsNull);
            Assert.Equal("LineString", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.Equal(3, g.STNumPoints().Value);
            Assert.True(g.HasZ);
            Assert.False(g.HasM);
            Assert.Equal(1, g.STNumGeometries().Value);

            Assert.Equal(0d, g.STPointN(1).STX.Value);
            Assert.Equal(1d, g.STPointN(1).STY.Value);
            Assert.Equal(1d, g.STPointN(1).Z.Value);
            Assert.True(g.STPointN(1).M.IsNull);

            Assert.Equal(3d, g.STPointN(2).STX.Value);
            Assert.Equal(2d, g.STPointN(2).STY.Value);
            Assert.Equal(2d, g.STPointN(2).Z.Value);
            Assert.True(g.STPointN(2).M.IsNull);

            var p3 = g.STPointN(3);
            Assert.Equal(4d, p3.STX.Value);
            Assert.Equal(5d, p3.STY.Value);
            Assert.True(p3.HasZ);
            Assert.True(p3.Z.IsNull); //3rd vertex is NaN and should therefore return Null here
            Assert.False(p3.HasM);
            Assert.True(p3.M.IsNull);
        }

        [Fact]
        public void TestGeometryCollection()
        {
            var coll = CreateBytes(4326, (byte)0x01, (byte)0x04,
               13, 0d, 4d, 2d,4d,3d,5d,0d,0d,0d,3d,3d,3d,3d,0d,0d,0d,1d,1d,2d,1d,2d,2d,1d,2d,1d,1d, //vertices
               4, (byte)0x01, 0, (byte)0x01, 1, (byte)0x02, 3, (byte)0x00, 8, //figures
               4, -1, 0, (byte) 0x07, 0,0, (byte)0x01, 0, 1, (byte)0x02, 0, 2, (byte)0x03 //shapes
               );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.False(g.IsNull);
            Assert.Equal("GeometryCollection", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.Equal(3, g.STNumGeometries());
            var p = g.STGeometryN(1);
            Assert.Equal("Point", p.STGeometryType());
            Assert.Equal(0d, p.STX.Value);
            Assert.Equal(4d, p.STY.Value);
            var l = g.STGeometryN(2);
            Assert.Equal("LineString", l.STGeometryType());
            Assert.Equal(2, l.STNumPoints());

            var pg = g.STGeometryN(3);
            Assert.Equal("Polygon", pg.STGeometryType());
            Assert.Equal(10, pg.STNumPoints());
            var extRing = pg.STExteriorRing();
            Assert.False(extRing.IsNull);
            Assert.Equal(5, extRing.STNumPoints());
            Assert.Equal(1, pg.STNumInteriorRing().Value);
            Assert.Equal(5, pg.STInteriorRingN(1).STNumPoints());
        }

        [Fact]
        public void TestCurvePolygon()
        {
            //TODO: Curve support not complete
            var coll = CreateBytes(4326, (byte)0x02, (byte)0x24,
              5, 0d,0d,2d,0d,2d,2d,0d,1d,0d,0d, //vertices
              1, (byte)0x03, 0, //figures
              1, -1, 0, (byte)0x10, //shapes
              3, (byte)0x02, (byte)0x00, (byte)0x03 //Segments
              );

            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.False(g.IsNull);
            Assert.Equal("CURVEPOLYGON", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            //TODO More asserts here
        }

        [Fact]
        public void TestSqlHiarchy1()
        {
            // The first child of the root node, with a logical representation of / 1 /, is represented as the following bit sequence:
            // 01011000
            // The first two bits, 01, are the L1 field, meaning that the first node has a label between 0(zero) and 3.The next two bits,
            // 01, are the O1 field and are interpreted as the integer 1.Adding this to the beginning of the range specified by the L1 yields 1.
            // The next bit, with the value 1, is the F1 field, which means that this is a "real" level, with 1 followed by a slash in the logical
            // representation.The final three bits, 000, are the W field, padding the representation to the nearest byte.
            byte[] bytes = { 0x58 }; //01011000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.Equal("/1/", hid.ToString());
        }

        [Fact]
        public void TestSqlHiarchy2()
        {
            // As a more complicated example, the node with logical representation / 1 / -2.18 / (the child with label - 2.18 of the child with label 1 of the root node) is represented as the following sequence of bits(a space has been inserted after every grouping of 8 bits to make the sequence easier to follow):
            // 01011001 11111011 00000101 01000000
            // The first three fields are the same as in the first example.That is, the first two bits(01) are the L1 field, the second two bits(01) are the O1 field, and the fifth bit(1) is the F1 field.This encodes the / 1 / portion of the logical representation.
            // The next 5 bits(00111) are the L2 field, so the next integer is between - 8 and - 1.The following 3 bits(111) are the O2 field, representing the offset 7 from the beginning of this range.Thus, the L2 and O2 fields together encode the integer - 1.The next bit(0) is the F2 field.Because it is 0(zero), this level is fake, and 1 has to be subtracted from the integer yielded by the L2 and O2 fields. Therefore, the L2, O2, and F2 fields together represent -2 in the logical representation of this node.
            // The next 3 bits(110) are the L3 field, so the next integer is between 16 and 79.The subsequent 8 bits(00001010) are the L4 field. Removing the anti - ambiguity bits from there(the third bit(0) and the fifth bit(1)) leaves 000010, which is the binary representation of 2.Thus, the integer encoded by the L3 and O3 fields is 16 + 2, which is 18.The next bit(1) is the F3 field, representing the slash(/) after the 18 in the logical representation.The final 6 bits(000000) are the W field, padding the physical representation to the nearest byte.
            byte[] bytes = { 0x59,0xFB,0x05,0x40 }; //01011001 11111011 00000101 01000000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.Equal("/1/-2.18/", hid.ToString());
        }

        private static byte[] CreateBytes(params object[] data)
        {
            using (var ms = new MemoryStream())
            {
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                foreach (var item in data)
                {
                    if (item is byte b)
                        bw.Write(b);
                    else if (item is int i)
                        bw.Write(i);
                    else if (item is double d)
                        bw.Write(d);
                    else
                        throw new ArgumentException();

                }
                return ms.ToArray();
            }
        }
    }
}