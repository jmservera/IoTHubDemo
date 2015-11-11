using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using DhtReadService;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        public object BitVector { get; private set; }

        [TestMethod]
        public void BitVector_starts_in_false()
        {
            BitVector v = new BitVector(40);
            for (uint i = 0; i < 40; i++)
            {
                Assert.IsFalse(v[i]);
            }
        }

        [TestMethod]
        public void BitVector_the_bit_is_set()
        {
            BitVector v = new BitVector(40);
            v[0] = true;
            Assert.IsTrue(v[0]);
            for (uint i = 1; i < 40; i++)
            {
                Assert.IsFalse(v[i]);
            }
        }

        [TestMethod]
        public void BitVector_any_bit_is_set_and_reset()
        {
            BitVector v = new BitVector(40);
            for (uint i = 1; i < 40; i++)
            {
                v[i] = true;
                Assert.IsTrue(v[i]);
            }

            for (uint i = 1; i < 40; i++)
            {
                v[i] = false;
                Assert.IsFalse(v[i]);
            }
        }

        [TestMethod]
        public void BitVector_conversion_gets_correct_array_length()
        {
            BitVector v = new BitVector(40);
            uint[] value = v.ToUintValues();
            Assert.AreEqual(2, value.Length);
            Assert.AreEqual((uint)0, value[0]);

            ulong[] longvalues = v.ToULongValues();
            Assert.AreEqual(1, longvalues.Length);
            Assert.AreEqual((ulong)0, longvalues[0]);
        }

        [TestMethod]
        public void BitVector_conversion_gets_correct_long_values()
        {
            BitVector v = new BitVector(40);

            v[0] = true;

            Assert.AreEqual((ulong)1, v.ToULongValues()[0]);

            v[1] = true;
            Assert.AreEqual((ulong)3, v.ToULongValues()[0]);

            v[0] = false;
            Assert.AreEqual((ulong)2, v.ToULongValues()[0]);

            v[1] = false;
            v[32] = true;
            Assert.AreEqual((ulong)4294967296, v.ToULongValues()[0]);

        }
        [TestMethod]
        public void BitVector_conversion_gets_correct_int_values()
        {
            BitVector v = new BitVector(40);

            v[0] = true;

            Assert.AreEqual((uint)1, v.ToUintValues()[0]);

            v[1] = true;
            Assert.AreEqual((uint)3, v.ToUintValues()[0]);

            v[0] = false;
            Assert.AreEqual((uint)2, v.ToUintValues()[0]);

            v[1] = false;
            v[32] = true;
            Assert.AreEqual((uint)0, v.ToUintValues()[0]);
            Assert.AreEqual((uint)1, v.ToUintValues()[1]);
        }
    }
}
