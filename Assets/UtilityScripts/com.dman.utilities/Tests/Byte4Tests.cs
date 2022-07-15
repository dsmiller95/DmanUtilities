using Dman.Utilities.Math;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dman.SceneSaveSystem.EditmodeTests
{
    public class Byte4Tests
    {
        [Test]
        public void ReadsOnesFromByte4()
        {
            var data = new byte4(0xFFFFFFFFU);
            Assert.AreEqual(0xFF, data[0]);
            Assert.AreEqual(0xFF, data[1]);
            Assert.AreEqual(0xFF, data[2]);
            Assert.AreEqual(0xFF, data[3]);
        }

        [Test]
        public void WritesOnesToByte4()
        {
            var data = new byte4();
            data[0] = 0xFF;
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = 0xFF;
            Assert.AreEqual(0xFF, data[0]);
            Assert.AreEqual(0xFF, data[1]);
            Assert.AreEqual(0xFF, data[2]);
            Assert.AreEqual(0xFF, data[3]);
        }


        [Test]
        public void ReadsZerosFromByte4()
        {
            var data = new byte4(0x00000000U);
            Assert.AreEqual(0x00, data[0]);
            Assert.AreEqual(0x00, data[1]);
            Assert.AreEqual(0x00, data[2]);
            Assert.AreEqual(0x00, data[3]);
        }

        [Test]
        public void WritesZeroesToByte4()
        {
            var data = new byte4(0xFFFFFFFFU);
            data[0] = 0x00;
            data[1] = 0x00;
            data[2] = 0x00;
            data[3] = 0x00;
            Assert.AreEqual(0x00, data[0]);
            Assert.AreEqual(0x00, data[1]);
            Assert.AreEqual(0x00, data[2]);
            Assert.AreEqual(0x00, data[3]);
        }
        [Test]
        public void ReadsVariosFromByte4()
        {
            var data = new byte4(0x0F153AF9U);
            Assert.AreEqual(0xF9, data[0]);
            Assert.AreEqual(0x3A, data[1]);
            Assert.AreEqual(0x15, data[2]);
            Assert.AreEqual(0x0F, data[3]);
        }

        [Test]
        public void WritesAlternatingToByte4()
        {
            var data = new byte4(0xFFFFFFFFU);
            data[0] = 0xAA;
            data[1] = 0x5A;
            data[2] = 0xFF;
            data[3] = 0xA5;
            Assert.AreEqual(0xAA, data[0]);
            Assert.AreEqual(0x5A, data[1]);
            Assert.AreEqual(0xFF, data[2]);
            Assert.AreEqual(0xA5, data[3]);
        }
    }
}
