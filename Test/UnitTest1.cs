using NUnit.Framework;
using System.IO;

namespace wh7r4bb17.AppSettings.Test
    {
    public class Tests
        {
        SettingsManager _manager;
        bool _res;

        [SetUp]
        public void Setup()
            {
            _res = false;
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
                _manager = new SettingsManager(filename);
                _res = true;
                }
            catch { }
            Assert.IsTrue(_res);
            }

        [Test]
        public void Test2_SetValues()
            {
            try
                {
                _manager.Set("General", "TestInteger", 2147483647);
                _manager.Set("General", "TestDouble", 12345.5);
                _manager.Set("General", "TestLong", 9223372036854775807);
                _manager.Set("General", "TestBool", true);
                _manager.Set("General", "TestByte", 255);
                _manager.Set("General", "TestString", "Hello, World!");
                _res = true;
                }
            catch { }
            Assert.IsTrue(_res);
            }

        [Test]
        public void Test3_GetBool()
            {
            bool boolValue = _manager.GetBool("General", "TestBool");
            Assert.AreEqual(boolValue, true);
            }

        [Test]
        public void Test4_GetInteger()
            {
            int intValue = _manager.GetInteger("General", "TestInteger");
            Assert.AreEqual(intValue, 2147483647);
            }

        [Test]
        public void Test5_GetDouble()
            {
            double doubleValue = _manager.GetDouble("General", "TestDouble");
            Assert.AreEqual(doubleValue, 12345.5);
            }

        [Test]
        public void Test6_GetLong()
            {
            long longValue = _manager.GetLong("General", "TestLong");
            Assert.AreEqual(longValue, 9223372036854775807);
            }

        [Test]
        public void Test7_GetByte()
            {
            byte byteValue = _manager.GetByte("General", "TestByte");
            Assert.AreEqual(byteValue, 255);
            }

        [Test]
        public void Test8_GetString()
            {
            string stringValue = _manager.Get("General", "TestString", "");
            Assert.AreEqual(stringValue, "Hello, World!");
            }

        [Test]
        public void Test9_SaveSettings()
            {
            try
                {
                _manager.SaveSettings();
                _res = true;
                }
            catch { }
            Assert.IsTrue(_res);
            }
        }
    }