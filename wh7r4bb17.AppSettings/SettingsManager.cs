using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace wh7r4bb17.AppSettings
    {
    /// <summary>
    /// Manages application settings with XML persistence.
    /// Supports hierarchical storage with groups and thread-safe operations.
    /// </summary>
    public class SettingsManager : IDisposable
        {
        #region Fields

        private readonly string _filePath;
        private readonly object _lockObject = new object();
        private readonly Dictionary<string, Dictionary<string, string>> _settings;
        private readonly char _valueSeparator;
        private string _xmlRootName = "Settings";

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Creates a new instance of the SettingsManager.
        /// </summary>
        /// <param name="fullFilePath">Full path of the XML document. If it does not exist, it will be created while saving.</param>
        /// <param name="valueSeparator">Character used to separate multiple values in a single setting. Default is semicolon (;).</param>
        /// <exception cref="ArgumentException">Thrown when fullFilePath is null or empty.</exception>
        public SettingsManager(string fullFilePath, char valueSeparator = ';')
            {
            if (string.IsNullOrWhiteSpace(fullFilePath))
                { throw new ArgumentException("File path must not be null or empty.", nameof(fullFilePath)); }

            _filePath = fullFilePath;
            _valueSeparator = valueSeparator;
            _settings = new Dictionary<string, Dictionary<string, string>>();
            _loadSettings();
            }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Adds a value to a separator-delimited list if it doesn't already exist.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="valueToAdd">The value to add to the list</param>
        /// <returns>True if the value was added, false if it already existed</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are null or empty.</exception>
        public bool AddValueToList(string groupName, string key, string valueToAdd)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            if (string.IsNullOrWhiteSpace(valueToAdd))
                { throw new ArgumentException("Value to add must not be null or empty.", nameof(valueToAdd)); }

            lock (_lockObject)
                {
                valueToAdd = valueToAdd.Trim();
                string currentValue = _getValue(groupName, key, string.Empty);

                var values = string.IsNullOrWhiteSpace(currentValue)
                    ? new List<string>()
                    : currentValue.Split(_valueSeparator)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrEmpty(v))
                        .ToList();

                if (values.Contains(valueToAdd, StringComparer.Ordinal))
                    { return false; }

                values.Add(valueToAdd);
                _setValue(groupName, key, string.Join(_valueSeparator.ToString(), values));

                return true;
                }
            }

        /// <summary>
        /// Clears all settings from memory. Does not affect the XML file until SaveSettings() is called.
        /// </summary>
        public void Clear()
            {
            lock (_lockObject)
                {
                _settings.Clear();
                }
            }

        /// <summary>
        /// Returns the string value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value or defaultValue if not found</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public string Get(string groupName, string key, string defaultValue = "")
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                return _getValue(groupName, key, defaultValue);
                }
            }

        /// <summary>
        /// Returns the bool value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value as bool or defaultValue if not found or invalid</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public bool GetBool(string groupName, string key, bool defaultValue = false)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string stringValue = _getValue(groupName, key);

                if (bool.TryParse(stringValue, out bool boolValue))
                    { return boolValue; }

                return defaultValue;
                }
            }

        /// <summary>
        /// Returns the byte value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value as byte or defaultValue if not found or invalid</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public byte GetByte(string groupName, string key, byte defaultValue = 0)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string stringValue = _getValue(groupName, key);

                if (byte.TryParse(stringValue, out byte byteValue))
                    { return byteValue; }

                return defaultValue;
                }
            }

        /// <summary>
        /// Returns the double value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value as double or defaultValue if not found or invalid</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public double GetDouble(string groupName, string key, double defaultValue = 0.0)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string stringValue = _getValue(groupName, key);

                if (double.TryParse(stringValue, out double doubleValue))
                    { return doubleValue; }

                return defaultValue;
                }
            }

        /// <summary>
        /// Returns the integer value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value as int or defaultValue if not found or invalid</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public int GetInteger(string groupName, string key, int defaultValue = 0)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string stringValue = _getValue(groupName, key);

                if (int.TryParse(stringValue, out int intValue))
                    { return intValue; }

                return defaultValue;
                }
            }

        /// <summary>
        /// Returns a list of all values from a separator-delimited setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <returns>List of values, empty list if setting not found</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public List<string> GetList(string groupName, string key)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string currentValue = _getValue(groupName, key, string.Empty);

                if (string.IsNullOrWhiteSpace(currentValue))
                    { return new List<string>(); }

                return currentValue.Split(_valueSeparator)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
                }
            }

        /// <summary>
        /// Returns the long value of the specified setting.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns>The setting value as long or defaultValue if not found or invalid</returns>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        public long GetLong(string groupName, string key, long defaultValue = 0)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            lock (_lockObject)
                {
                string stringValue = _getValue(groupName, key);

                if (long.TryParse(stringValue, out long longValue))
                    { return longValue; }

                return defaultValue;
                }
            }

        /// <summary>
        /// Reloads settings from the XML file, discarding any unsaved changes.
        /// </summary>
        public void Reload()
            {
            lock (_lockObject)
                {
                _settings.Clear();
                _loadSettings();
                }
            }

        /// <summary>
        /// Removes a value from a separator-delimited list if it exists.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="valueToRemove">The value to remove from the list</param>
        /// <returns>True if the value was removed, false if it didn't exist</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are null or empty.</exception>
        public bool RemoveValueFromList(string groupName, string key, string valueToRemove)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            if (string.IsNullOrWhiteSpace(valueToRemove))
                { throw new ArgumentException("Value to remove must not be null or empty.", nameof(valueToRemove)); }

            lock (_lockObject)
                {
                valueToRemove = valueToRemove.Trim();
                string currentValue = _getValue(groupName, key, string.Empty);

                if (string.IsNullOrWhiteSpace(currentValue))
                    { return false; }

                var values = currentValue.Split(_valueSeparator)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                bool removed = values.Remove(valueToRemove);

                if (removed)
                    {
                    string newValue = values.Count > 0
                        ? string.Join(_valueSeparator.ToString(), values)
                        : string.Empty;
                    _setValue(groupName, key, newValue);
                    }

                return removed;
                }
            }

        /// <summary>
        /// Saves the settings to the XML document. If the file does not exist, it will be created.
        /// </summary>
        /// <param name="xmlRootName">Name of the XML root element. Used only for new documents.</param>
        /// <exception cref="ArgumentException">Thrown when xmlRootName is null or empty.</exception>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        public void SaveSettings(string xmlRootName = "Settings")
            {
            if (string.IsNullOrWhiteSpace(xmlRootName))
                { throw new ArgumentException("XML root name must not be null or empty.", nameof(xmlRootName)); }

            lock (_lockObject)
                {
                try
                    {
                    if (_xmlRootName == "Settings")
                        { _xmlRootName = xmlRootName; }

                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(_filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        { Directory.CreateDirectory(directory); }

                    var xmlDoc = new XmlDocument();
                    xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));

                    XmlElement rootElement = xmlDoc.CreateElement(_xmlRootName);
                    xmlDoc.AppendChild(rootElement);

                    foreach (var group in _settings)
                        {
                        XmlElement groupElement = xmlDoc.CreateElement(SanitizeXmlName(group.Key));

                        foreach (var kvp in group.Value)
                            {
                            XmlElement settingElement = xmlDoc.CreateElement(SanitizeXmlName(kvp.Key));
                            settingElement.InnerText = kvp.Value ?? string.Empty;
                            groupElement.AppendChild(settingElement);
                            }

                        rootElement.AppendChild(groupElement);
                        }

                    xmlDoc.Save(_filePath);
                    }
                catch (IOException ex)
                    {
                    Debug.WriteLine($"IO error while saving settings to {_filePath}: {ex.Message}");
                    throw;
                    }
                catch (XmlException ex)
                    {
                    Debug.WriteLine($"XML error while saving settings: {ex.Message}");
                    throw;
                    }
                }
            }

        /// <summary>
        /// Sets a setting to a settings group.
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="value">The value to set</param>
        /// <exception cref="ArgumentException">Thrown when groupName or key are null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public void Set(string groupName, string key, object value)
            {
            if (string.IsNullOrWhiteSpace(groupName))
                { throw new ArgumentException("Group name must not be null or empty.", nameof(groupName)); }

            if (string.IsNullOrWhiteSpace(key))
                { throw new ArgumentException("Key must not be null or empty.", nameof(key)); }

            if (value == null)
                { throw new ArgumentNullException(nameof(value)); }

            lock (_lockObject)
                {
                string valueStr = value.ToString();
                _setValue(groupName, key, valueStr);
                }
            }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sanitizes XML element names by removing invalid characters.
        /// </summary>
        private static string SanitizeXmlName(string name)
            {
            if (string.IsNullOrEmpty(name))
                { return "Element"; }

            string sanitized = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-.]", "_");
            if (string.IsNullOrEmpty(sanitized))
                { return "Element"; }

            if (char.IsDigit(sanitized[0]) || sanitized[0] == '-' || sanitized[0] == '.')
                { sanitized = "_" + sanitized; }

            return sanitized;
            }

        /// <summary>
        /// Gets a setting value from the in-memory dictionary.
        /// </summary>
        private string _getValue(string groupName, string key, string defaultValue = "")
            {
            if (_settings.TryGetValue(groupName, out Dictionary<string, string> group))
                {
                if (group.TryGetValue(key, out string value))
                    { return value ?? defaultValue; }
                }
            return defaultValue;
            }

        /// <summary>
        /// Loads settings from the XML file into memory.
        /// </summary>
        private void _loadSettings()
            {
            if (!File.Exists(_filePath))
                { return; }

            try
                {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(_filePath);

                if (xmlDoc.DocumentElement == null)
                    { return; }

                _xmlRootName = xmlDoc.DocumentElement.Name ?? "Settings";

                foreach (XmlElement groupElement in xmlDoc.DocumentElement.ChildNodes.OfType<XmlElement>())
                    {
                    string groupName = groupElement.Name;

                    if (!_settings.ContainsKey(groupName))
                        { _settings[groupName] = new Dictionary<string, string>(); }

                    foreach (XmlElement settingElement in groupElement.ChildNodes.OfType<XmlElement>())
                        {
                        string key = settingElement.Name;
                        string value = settingElement.InnerText ?? string.Empty;
                        _settings[groupName][key] = value;
                        }
                    }
                }
            catch (XmlException ex)
                {
                Debug.WriteLine($"XML load error in {_filePath}: {ex.Message}");
                }
            catch (IOException ex)
                {
                Debug.WriteLine($"IO error while loading {_filePath}: {ex.Message}");
                }
            }

        /// <summary>
        /// Sets a setting value in the in-memory dictionary.
        /// </summary>
        private void _setValue(string groupName, string key, string value)
            {
            if (!_settings.ContainsKey(groupName))
                { _settings[groupName] = new Dictionary<string, string>(); }

            _settings[groupName][key] = value ?? string.Empty;
            }

        #endregion Private Methods

        #region IDisposable

        /// <summary>
        /// Disposes the SettingsManager resources.
        /// </summary>
        public void Dispose()
            {
            GC.SuppressFinalize(this);
            }

        #endregion IDisposable
        }
    }