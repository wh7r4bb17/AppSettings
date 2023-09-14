using NUnit.Framework;
using System.IO;

namespace wh7r4bb17.AppSettings.Test
    {
    public class Tests
        {
        SettingsManager manager;
        bool res;

        [SetUp]
        public void Setup()
            {
            res = false;
            if (!Directory.Exists("C:\\temp"))
                {
                Directory.CreateDirectory("C:\\temp");
                }
            }

        [Test]
        public void Test1_Create()
            {
            string filename = "C:\\temp\\Test_Settings.xml";
            try
                {
                manager = new SettingsManager(filename);
                res = true;
                }
            catch { }
            Assert.IsTrue(res);
            }

        [Test]
        public void Test2_SetValues()
            {
            try
                {
                manager.Set("General", "TestInteger", 2147483647);
                manager.Set("General", "TestDouble", 12345.5);
                manager.Set("General", "TestLong", 9223372036854775807);
                manager.Set("General", "TestBool", true);
                manager.Set("General", "TestByte", 255);
                manager.Set("General", "TestString", "Hello, World!");
                res = true;
                }
            catch { }
            Assert.IsTrue(res);
            }

        [Test]
        public void Test3_GetBool()
            {
            bool boolValue = manager.GetBool("General", "TestBool");
            Assert.AreEqual(boolValue, true);
            }

        [Test]
        public void Test4_GetInteger()
            {
            int intValue = manager.GetInteger("General", "TestInteger");
            Assert.AreEqual(intValue, 2147483647);
            }

        [Test]
        public void Test5_GetDouble()
            {
            double doubleValue = manager.GetDouble("General", "TestDouble");
            Assert.AreEqual(doubleValue, 12345.5);
            }

        [Test]
        public void Test6_GetLong()
            {
            long longValue = manager.GetLong("General", "TestLong");
            Assert.AreEqual(longValue, 9223372036854775807);
            }

        [Test]
        public void Test7_GetByte()
            {
            byte byteValue = manager.GetByte("General", "TestByte");
            Assert.AreEqual(byteValue, 255);
            }

        [Test]
        public void Test8_GetString()
            {
            string stringValue = manager.Get("General", "TestString", "");
            Assert.AreEqual(stringValue, "Hello, World!");
            }

        [Test]
        public void Test9_SaveSettings()
            {
            try
                {
                manager.SaveSettings();
                res = true;
                }
            catch { }
            Assert.IsTrue(res);
            }
        }
    }