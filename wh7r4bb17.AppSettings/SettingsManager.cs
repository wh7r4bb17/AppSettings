using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace wh7r4bb17.AppSettings
    {
    public class SettingsManager
        {
        #region Fields
        private readonly string _filePath;
        private string _xmlRootName;
        private readonly Dictionary<string, Dictionary<string, string>> _settings;

        #endregion Fields

        #region Constructor
        /// <summary>
        /// create a new instance of the SettingsManager.
        /// </summary>
        /// <param name="FullFilePath">Full path of the xml document. If it does not exist, it will be created while saving</param>
        public SettingsManager(string FullFilePath)
            {
            _filePath = FullFilePath;
            _settings = new Dictionary<string, Dictionary<string, string>>();
            _loadSettings();
            }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Sets a Setting to a Settings Group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group. Also the Name of the xml Subnode name</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="value">The Value to set</param>
        public void Set(string groupName, string key, object value)
            {
            string valueStr = value.ToString();
            _setValue(groupName, key, valueStr);
            }


        /// <summary>
        /// Returns the string Value of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public string Get(string groupName, string key, string defaultValue = "")
            {
            return _getValue(groupName, key, defaultValue);
            }


        /// <summary>
        /// Returns the bool Value of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public bool GetBool(string groupName, string key, bool defaultValue = false)
            {
            string stringValue = _getValue(groupName, key);

            if (bool.TryParse(stringValue, out bool boolValue))
                {
                return boolValue;
                }
            return defaultValue;
            }


        /// <summary>
        /// Returns the Integer Value of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public int GetInteger(string groupName, string key, int defaultValue = 0)
            {
            string stringValue = _getValue(groupName, key);

            if (int.TryParse(stringValue, out int intValue))
                {
                return intValue;
                }
            return defaultValue;
            }


        /// <summary>
        /// Returns the double Value of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public double GetDouble(string groupName, string key, double defaultValue = 0.0)
            {
            string stringValue = _getValue(groupName, key);

            if (double.TryParse(stringValue, out double doubleValue))
                {
                return doubleValue;
                }
            return defaultValue;
            }


        /// <summary>
        /// Returns the long Value of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public long GetLong(string groupName, string key, long defaultValue = 0)
            {
            string stringValue = _getValue(groupName, key);

            if (long.TryParse(stringValue, out long longValue))
                {
                return longValue;
                }
            return defaultValue;
            }


        /// <summary>
        /// Returns the Value as byte of the specified group
        /// </summary>
        /// <param name="groupName">The name of the Settings Group.</param>
        /// <param name="key">The Setting Name</param>
        /// <param name="defaultValue">Value which is returned if no setting is present.</param>
        /// <returns></returns>
        public byte GetByte(string groupName, string key, byte defaultValue = 0)
            {
            string stringValue = _getValue(groupName, key);

            if (byte.TryParse(stringValue, out byte byteValue))
                {
                return byteValue;
                }
            return defaultValue;
            }


        /// <summary>
        ///  Save the Settings to Xml document. If the xml not exist, it will be created.
        /// </summary>
        /// <param name="xmlRootName">Name of the xml root element, if it's a new document or there's no root element</param>
        public void SaveSettings(string xmlRootName = "Settings")
            {
            _xmlRootName = _xmlRootName ?? xmlRootName;
            XmlDocument XmlDoc = new XmlDocument();
            XmlElement RootElement = XmlDoc.CreateElement(_xmlRootName);
            XmlDoc.AppendChild(RootElement);

            foreach (var _group in _settings)
                {
                XmlElement groupElement = XmlDoc.CreateElement(_group.Key);

                foreach (var kvp in _group.Value)
                    {
                    XmlElement settingElement = XmlDoc.CreateElement(kvp.Key);
                    settingElement.InnerText = kvp.Value;
                    groupElement.AppendChild(settingElement);
                    }
                RootElement.AppendChild(groupElement);
                }

            if (!File.Exists(_filePath))
                {
                string directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory))
                    {
                    Directory.CreateDirectory(directory);
                    }
                }
            XmlDoc.Save(_filePath);
            }

        #endregion Methods

        #region private Methods
        void _loadSettings()
            {
            if (File.Exists(_filePath))
                {
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(_filePath);
                _xmlRootName = XmlDoc.DocumentElement.Name ?? "Settings";

                foreach (XmlElement groupElement in XmlDoc.DocumentElement.ChildNodes)
                    {
                    string groupName = groupElement.Name;
                    _settings[groupName] = new Dictionary<string, string>();

                    foreach (XmlElement settingElement in groupElement.ChildNodes)
                        {
                        string key = settingElement.Name;
                        string value = settingElement.InnerText;
                        _settings[groupName][key] = value;
                        }
                    }
                }
            }

        string _getValue(string groupName, string key, string defaultValue = "")
            {
            if (_settings.TryGetValue(groupName, out Dictionary<string, string> group))
                {
                if (group.TryGetValue(key, out string value))
                    {
                    return value;
                    }
                }
            return defaultValue;
            }

        void _setValue(string groupName, string key, string value)
            {
            if (!_settings.ContainsKey(groupName))
                {
                _settings[groupName] = new Dictionary<string, string>();
                }
            _settings[groupName][key] = value;
            }

        #endregion private Methods
        }
    }