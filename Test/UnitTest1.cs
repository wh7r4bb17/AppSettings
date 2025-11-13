using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace wh7r4bb17.AppSettings.Test
    {
    /// <summary>
    /// Concurrency Tests - Each test gets a new XML file
    /// These tests verify thread-safe operations.
    /// </summary>
    [TestFixture]
    public class SettingsManagerConcurrencyTests
        {
        #region Fields

        private SettingsManager _manager;
        private string _testDirectory;
        private string _testFilePath;

        #endregion Fields

        #region Setup & Teardown

        [OneTimeSetUp]
        public void OneTimeSetUp()
            {
            _testDirectory = Path.Combine(Path.GetTempPath(), "SettingsManagerTest_Concurrency");
            if (!Directory.Exists(_testDirectory))
                { Directory.CreateDirectory(_testDirectory); }
            }

        [OneTimeTearDown]
        public void OneTimeTearDown()
            {
            if (Directory.Exists(_testDirectory))
                { Directory.Delete(_testDirectory, true); }
            }

        [SetUp]
        public void SetUp()
            {
            _testFilePath = Path.Combine(_testDirectory, $"TestSettings_{Guid.NewGuid():N}.xml");
            _manager = null;
            }

        [TearDown]
        public void TearDown()
            {
            _manager?.Dispose();

            if (File.Exists(_testFilePath))
                { File.Delete(_testFilePath); }
            }

        #endregion Setup & Teardown

        #region Concurrency Tests

        [Test]
        public void ConcurrentListOperations_NoExceptions()
            {
            _manager = new SettingsManager(_testFilePath);
            var tasks = new List<System.Threading.Tasks.Task>();

            for (int i = 0; i < 15; i++)
                {
                int index = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    _manager.AddValueToList("Group", "List", $"Value{index}");
                }));
                }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            var result = _manager.GetList("Group", "List");
            Assert.AreEqual(15, result.Count);
            }

        [Test]
        public void ConcurrentMixedOperations_NoExceptions()
            {
            _manager = new SettingsManager(_testFilePath);
            var tasks = new List<System.Threading.Tasks.Task>();

            for (int i = 0; i < 20; i++)
                {
                int index = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    if (index % 3 == 0)
                        { _manager.Set($"Group{index}", $"Key{index}", $"Value{index}"); }
                    else if (index % 3 == 1)
                        { _manager.Get($"Group{index % 10}", $"Key{index % 10}"); }
                    else
                        { _manager.AddValueToList($"Group{index}", "List", $"Item{index}"); }
                }));
                }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            Assert.Pass("No exceptions during concurrent mixed operations");
            }

        [Test]
        public void ConcurrentReads_NoExceptions()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.Set("Group", "Key", "Value");
            var tasks = new List<System.Threading.Tasks.Task>();

            for (int i = 0; i < 20; i++)
                {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    var value = _manager.Get("Group", "Key");
                }));
                }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            Assert.Pass("No exceptions during concurrent reads");
            }

        [Test]
        public void ConcurrentWrites_NoExceptions()
            {
            _manager = new SettingsManager(_testFilePath);
            var tasks = new List<System.Threading.Tasks.Task>();

            for (int i = 0; i < 10; i++)
                {
                int index = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    _manager.Set($"Group{index}", $"Key{index}", $"Value{index}");
                }));
                }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            Assert.Pass("No exceptions during concurrent writes");
            }

        #endregion Concurrency Tests
        }

    /// <summary>
    /// Isolated Tests - Each test gets a new XML file
    /// These tests are independent and can run in parallel.
    /// </summary>
    [TestFixture]
    public class SettingsManagerIsolatedTests
        {
        #region Fields

        private SettingsManager _manager;
        private string _testDirectory;
        private string _testFilePath;

        #endregion Fields

        #region Setup & Teardown

        [OneTimeSetUp]
        public void OneTimeSetUp()
            {
            _testDirectory = Path.Combine(Path.GetTempPath(), "SettingsManagerTest_Isolated");
            if (!Directory.Exists(_testDirectory))
                { Directory.CreateDirectory(_testDirectory); }
            }

        [OneTimeTearDown]
        public void OneTimeTearDown()
            {
            if (Directory.Exists(_testDirectory))
                { Directory.Delete(_testDirectory, true); }
            }

        [SetUp]
        public void SetUp()
            {
            _testFilePath = Path.Combine(_testDirectory, $"TestSettings_{Guid.NewGuid():N}.xml");
            _manager = null;
            }

        [TearDown]
        public void TearDown()
            {
            _manager?.Dispose();

            if (File.Exists(_testFilePath))
                { File.Delete(_testFilePath); }
            }

        #endregion Setup & Teardown

        #region Foundation Tests (Must Pass First)

        [Test, Order(5)]
        public void Constructor_WithCustomSeparator_UsesCustomSeparator()
            {
            _manager = new SettingsManager(_testFilePath, '|');

            _manager.AddValueToList("Test", "List", "Value1");
            _manager.AddValueToList("Test", "List", "Value2");
            _manager.SaveSettings();

            var values = _manager.GetList("Test", "List");
            Assert.AreEqual(2, values.Count);
            Assert.Contains("Value1", values);
            Assert.Contains("Value2", values);
            }

        [Test, Order(4)]
        public void Constructor_WithEmptyPath_ThrowsArgumentException()
            {
            Assert.Throws<ArgumentException>(() => new SettingsManager(string.Empty));
            }

        [Test, Order(3)]
        public void Constructor_WithNullPath_ThrowsArgumentException()
            {
            Assert.Throws<ArgumentException>(() => new SettingsManager(null));
            }

        [Test, Order(2)]
        public void Constructor_WithValidPath_CreatesFileOnFirstSave()
            {
            _manager = new SettingsManager(_testFilePath);
            Assert.IsFalse(File.Exists(_testFilePath));

            _manager.SaveSettings();

            Assert.IsTrue(File.Exists(_testFilePath));
            }

        [Test, Order(1)]
        public void Constructor_WithValidPath_CreatesInstance()
            {
            _manager = new SettingsManager(_testFilePath);

            Assert.IsNotNull(_manager);
            }

        #endregion Foundation Tests (Must Pass First)

        #region Set & Get Tests

        [Test, Order(13)]
        public void Get_WithDefaultValue_ReturnsDefaultWhenKeyNotFound()
            {
            _manager = new SettingsManager(_testFilePath);
            string defaultValue = "DefaultValue";

            string result = _manager.Get("NonExistent", "NonExistent", defaultValue);

            Assert.AreEqual(defaultValue, result);
            }

        [Test, Order(14)]
        public void Set_MultipleGroups_StoredSeparately()
            {
            _manager = new SettingsManager(_testFilePath);

            _manager.Set("Group1", "Key", "Value1");
            _manager.Set("Group2", "Key", "Value2");
            var result1 = _manager.Get("Group1", "Key");
            var result2 = _manager.Get("Group2", "Key");

            Assert.AreEqual("Value1", result1);
            Assert.AreEqual("Value2", result2);
            }

        [Test, Order(11)]
        public void Set_WithNullGroupName_ThrowsArgumentException()
            {
            _manager = new SettingsManager(_testFilePath);

            Assert.Throws<ArgumentException>(() => _manager.Set(null, "Key", "Value"));
            }

        [Test, Order(12)]
        public void Set_WithNullValue_ThrowsArgumentNullException()
            {
            _manager = new SettingsManager(_testFilePath);

            Assert.Throws<ArgumentNullException>(() => _manager.Set("Group", "Key", null));
            }

        [Test, Order(10)]
        public void Set_WithValidParameters_StoresValue()
            {
            _manager = new SettingsManager(_testFilePath);

            _manager.Set("TestGroup", "TestKey", "TestValue");

            var result = _manager.Get("TestGroup", "TestKey");
            Assert.AreEqual("TestValue", result);
            }

        #endregion Set & Get Tests

        #region Type-Specific Tests

        [Test, Order(26)]
        public void GetInteger_WithInvalidValue_ReturnsDefaultValue()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.Set("Test", "InvalidInt", "NotAnInteger");

            int result = _manager.GetInteger("Test", "InvalidInt", 42);

            Assert.AreEqual(42, result);
            }

        [Test, Order(23)]
        public void SetAndGet_Bool_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);

            _manager.Set("General", "TestBool", true);
            bool actualTrue = _manager.GetBool("General", "TestBool");
            Assert.IsTrue(actualTrue);

            _manager.Set("General", "TestBool", false);
            bool actualFalse = _manager.GetBool("General", "TestBool");
            Assert.IsFalse(actualFalse);
            }

        [Test, Order(24)]
        public void SetAndGet_Byte_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            byte expectedValue = 255;

            _manager.Set("General", "TestByte", expectedValue);
            byte actualValue = _manager.GetByte("General", "TestByte");

            Assert.AreEqual(expectedValue, actualValue);
            }

        [Test, Order(21)]
        public void SetAndGet_Double_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            double expectedValue = 12345.6789;

            _manager.Set("General", "TestDouble", expectedValue);
            double actualValue = _manager.GetDouble("General", "TestDouble");

            Assert.AreEqual(expectedValue, actualValue, 0.0001);
            }

        [Test, Order(20)]
        public void SetAndGet_Integer_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            int expectedValue = 2147483647;

            _manager.Set("General", "TestInteger", expectedValue);
            int actualValue = _manager.GetInteger("General", "TestInteger");

            Assert.AreEqual(expectedValue, actualValue);
            }

        [Test, Order(22)]
        public void SetAndGet_Long_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            long expectedValue = 9223372036854775807;

            _manager.Set("General", "TestLong", expectedValue);
            long actualValue = _manager.GetLong("General", "TestLong");

            Assert.AreEqual(expectedValue, actualValue);
            }

        [Test, Order(25)]
        public void SetAndGet_String_PreservesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            string expectedValue = "Hello, World!";

            _manager.Set("General", "TestString", expectedValue);
            string actualValue = _manager.Get("General", "TestString");

            Assert.AreEqual(expectedValue, actualValue);
            }

        #endregion Type-Specific Tests

        #region List Operations Tests

        [Test, Order(31)]
        public void AddValueToList_WithDuplicateValue_ReturnsFalse()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.AddValueToList("Group", "List", "Value1");

            bool result = _manager.AddValueToList("Group", "List", "Value1");

            Assert.IsFalse(result);
            }

        [Test, Order(32)]
        public void AddValueToList_WithMultipleValues_StoresAllValues()
            {
            _manager = new SettingsManager(_testFilePath);

            _manager.AddValueToList("Group", "List", "Value1");
            _manager.AddValueToList("Group", "List", "Value2");
            _manager.AddValueToList("Group", "List", "Value3");
            var result = _manager.GetList("Group", "List");

            Assert.AreEqual(3, result.Count);
            Assert.Contains("Value1", result);
            Assert.Contains("Value2", result);
            Assert.Contains("Value3", result);
            }

        [Test, Order(37)]
        public void AddValueToList_WithNullGroup_ThrowsArgumentException()
            {
            _manager = new SettingsManager(_testFilePath);

            Assert.Throws<ArgumentException>(() => _manager.AddValueToList(null, "Key", "Value"));
            }

        [Test, Order(38)]
        public void AddValueToList_WithNullValue_ThrowsArgumentException()
            {
            _manager = new SettingsManager(_testFilePath);

            Assert.Throws<ArgumentException>(() => _manager.AddValueToList("Group", "Key", null));
            }

        [Test, Order(30)]
        public void AddValueToList_WithValidParameters_AddsValue()
            {
            _manager = new SettingsManager(_testFilePath);

            bool result = _manager.AddValueToList("Group", "List", "Value1");

            Assert.IsTrue(result);
            }

        [Test, Order(33)]
        public void GetList_WithNonExistentKey_ReturnsEmptyList()
            {
            _manager = new SettingsManager(_testFilePath);

            var result = _manager.GetList("NonExistent", "NonExistent");

            Assert.IsEmpty(result);
            }

        [Test, Order(36)]
        public void RemoveValueFromList_RemovingLastValue_ClearsKey()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.AddValueToList("Group", "List", "Value1");

            _manager.RemoveValueFromList("Group", "List", "Value1");
            var result = _manager.GetList("Group", "List");

            Assert.IsEmpty(result);
            }

        [Test, Order(34)]
        public void RemoveValueFromList_WithExistingValue_RemovesValue()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.AddValueToList("Group", "List", "Value1");
            _manager.AddValueToList("Group", "List", "Value2");

            bool result = _manager.RemoveValueFromList("Group", "List", "Value1");
            var remaining = _manager.GetList("Group", "List");

            Assert.IsTrue(result);
            Assert.AreEqual(1, remaining.Count);
            Assert.Contains("Value2", remaining);
            }

        [Test, Order(35)]
        public void RemoveValueFromList_WithNonExistentValue_ReturnsFalse()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.AddValueToList("Group", "List", "Value1");

            bool result = _manager.RemoveValueFromList("Group", "List", "NonExistent");

            Assert.IsFalse(result);
            }

        #endregion List Operations Tests

        #region Utility Tests

        [Test, Order(50)]
        public void Clear_RemovesAllSettings()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.Set("Group1", "Key1", "Value1");
            _manager.Set("Group2", "Key2", "Value2");

            _manager.Clear();
            var result1 = _manager.Get("Group1", "Key1", "DEFAULT");
            var result2 = _manager.Get("Group2", "Key2", "DEFAULT");

            Assert.AreEqual("DEFAULT", result1);
            Assert.AreEqual("DEFAULT", result2);
            }

        [Test, Order(51)]
        public void Reload_DiscardsPendingChanges()
            {
            _manager = new SettingsManager(_testFilePath);
            _manager.Set("Group1", "Key1", "OriginalValue");
            _manager.SaveSettings();

            _manager.Set("Group1", "Key1", "NewValue");
            _manager.Reload();
            string result = _manager.Get("Group1", "Key1");

            Assert.AreEqual("OriginalValue", result);
            }

        [Test, Order(52)]
        public void ThreadSafety_ConcurrentOperations_NoExceptions()
            {
            _manager = new SettingsManager(_testFilePath);
            var tasks = new List<System.Threading.Tasks.Task>();

            for (int i = 0; i < 10; i++)
                {
                int index = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    _manager.Set($"Group{index}", $"Key{index}", $"Value{index}");
                    _manager.Get($"Group{index}", $"Key{index}");
                }));
                }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            Assert.Pass("No exceptions occurred during concurrent operations");
            }

        #endregion Utility Tests
        }

    /// <summary>
    /// Persistence Tests - All tests share one XML file
    /// These tests verify data persistence and file handling.
    /// </summary>
    [TestFixture]
    public class SettingsManagerPersistenceTests
        {
        #region Fields

        private SettingsManager _manager;
        private string _sharedFilePath;
        private string _testDirectory;

        #endregion Fields

        #region Setup & Teardown

        [OneTimeSetUp]
        public void OneTimeSetUp()
            {
            _testDirectory = Path.Combine(Path.GetTempPath(), "SettingsManagerTest_Persistence");
            if (!Directory.Exists(_testDirectory))
                { Directory.CreateDirectory(_testDirectory); }

            _sharedFilePath = Path.Combine(_testDirectory, "SharedSettings.xml");

            if (File.Exists(_sharedFilePath))
                { File.Delete(_sharedFilePath); }
            }

        [OneTimeTearDown]
        public void OneTimeTearDown()
            {
            if (File.Exists(_sharedFilePath))
                { File.Delete(_sharedFilePath); }

            if (Directory.Exists(_testDirectory))
                { Directory.Delete(_testDirectory, true); }
            }

        [SetUp]
        public void SetUp()
            {
            _manager = new SettingsManager(_sharedFilePath);
            }

        [TearDown]
        public void TearDown()
            {
            _manager?.Dispose();
            }

        #endregion Setup & Teardown

        #region Persistence Tests

        [Test, Order(6)]
        public void CustomRootName_PersistsAcrossLoads()
            {
            _manager.Set("Custom", "Key", "Value");

            _manager.SaveSettings("CustomRoot");
            var newManager = new SettingsManager(_sharedFilePath);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_sharedFilePath);
            Assert.AreEqual("CustomRoot", xmlDoc.DocumentElement.Name);

            newManager.Dispose();
            }

        [Test, Order(5)]
        public void FileStructure_IsValid()
            {
            _manager.Set("Group1", "Key1", "Value1");
            _manager.AddValueToList("Group1", "List", "Item1");
            _manager.AddValueToList("Group1", "List", "Item2");
            _manager.SaveSettings();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_sharedFilePath);

            Assert.IsNotNull(xmlDoc.DocumentElement);
            Assert.AreEqual("Settings", xmlDoc.DocumentElement.Name);
            Assert.IsTrue(xmlDoc.DocumentElement.HasChildNodes);
            }

        [Test, Order(2)]
        public void LoadExistingFile_RestoresData()
            {
            var newManager = new SettingsManager(_sharedFilePath);

            string value1 = newManager.Get("TestGroup", "Key1");
            int value2 = newManager.GetInteger("TestGroup", "Key2");

            Assert.AreEqual("Value1", value1);
            Assert.AreEqual(42, value2);

            newManager.Dispose();
            }

        [Test, Order(4)]
        public void MultipleInstances_ShareData()
            {
            _manager.Set("Shared", "SharedKey", "SharedValue");
            _manager.SaveSettings();

            var manager2 = new SettingsManager(_sharedFilePath);
            string result = manager2.Get("Shared", "SharedKey");

            Assert.AreEqual("SharedValue", result);

            manager2.Dispose();
            }

        [Test, Order(3)]
        public void Reload_RefreshesFromDisk()
            {
            _manager.Set("TestGroup", "Key3", "Value3");
            _manager.SaveSettings();

            var externalManager = new SettingsManager(_sharedFilePath);
            externalManager.Set("TestGroup", "Key3", "ModifiedValue");
            externalManager.SaveSettings();
            externalManager.Dispose();

            _manager.Reload();
            string result = _manager.Get("TestGroup", "Key3");

            Assert.AreEqual("ModifiedValue", result);
            }

        [Test, Order(1)]
        public void SaveAndLoad_PreservesData()
            {
            _manager.Set("TestGroup", "Key1", "Value1");
            _manager.Set("TestGroup", "Key2", 42);

            _manager.SaveSettings();

            Assert.IsTrue(File.Exists(_sharedFilePath));
            var content = File.ReadAllText(_sharedFilePath);
            Assert.IsTrue(content.Contains("TestGroup"));
            Assert.IsTrue(content.Contains("Key1"));
            }

        [Test, Order(7)]
        public void SaveSettings_WithNullRootName_ThrowsArgumentException()
            {
            Assert.Throws<ArgumentException>(() => _manager.SaveSettings(null));
            }

        #endregion Persistence Tests
        }
    }