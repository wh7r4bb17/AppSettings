# About

wh7r4bb17.AppSettings is a helper class to read and save application settings from/to an XML document.
The SettingsManager can set values as strings and retrieve values as string, bool, int, double, long, and byte.

The class is provided in **.NET Standard 2.0** and is compatible with:

- .NET Framework 4.7.2+
- .NET Framework 4.8, 4.8.1
- .NET 6, 7, 8, 9

## Features

- **Hierarchical Storage**: Organize settings in groups with key-value pairs
- **Type Support**: Automatic conversion between string storage and various .NET types
- **List Support**: Store multiple values in a single setting using a configurable separator (default: semicolon)
- **Thread-Safe**: All operations are protected with locks for concurrent access
- **XML Persistence**: Automatic XML file creation and management
- **Custom Separators**: Configure the delimiter for list values per instance
- **Error Handling**: Comprehensive exception handling with meaningful error messages
- **IDisposable**: Proper resource cleanup support

## Supported Data Types

- `string` - Text values
- `bool` - Boolean values
- `int` - Integer values
- `double` - Floating-point values
- `long` - Long integer values
- `byte` - Byte values
- `List<string>` - Delimited list of values

## Installation

Add the `SettingsManager.cs` file to your project:

```bash
# Copy the file to your project
cp SettingsManager.cs YourProject/
```

Or reference as NuGet package:

```bash
dotnet add package wh7r4bb17.AppSettings
```

## Usage

### Basic Usage

```csharp
using wh7r4bb17.AppSettings;

// Create instance with default settings
var settings = new SettingsManager(@"C:\AppData\settings.xml");

// Set values
settings.Set("General", "ApplicationName", "MyApp");
settings.Set("General", "Version", 1.0);
settings.Set("General", "Debug", true);

// Get values
string appName = settings.Get("General", "ApplicationName");
double version = settings.GetDouble("General", "Version");
bool debugMode = settings.GetBool("General", "Debug");

// Save to XML file
settings.SaveSettings();

// Cleanup
settings.Dispose();
```

### Using Custom Separator

```csharp
using wh7r4bb17.AppSettings;

// Create instance with pipe separator
var settings = new SettingsManager(@"C:\AppData\settings.xml", '|');

// Add values to list
settings.AddValueToList("RecentFiles", "Files", "C:\file1.txt");
settings.AddValueToList("RecentFiles", "Files", "C:\file2.txt");

// Get list values
List<string> files = settings.GetList("RecentFiles", "Files");
// Result: ["C:\file1.txt", "C:\file2.txt"]

settings.SaveSettings();
```

### List Operations

```csharp
using wh7r4bb17.AppSettings;

var settings = new SettingsManager(@"C:\AppData\settings.xml");

// Add values to a list
bool added = settings.AddValueToList("Settings", "IgnoredExtensions", "*.tmp");
added = settings.AddValueToList("Settings", "IgnoredExtensions", "*.bak");

// Returns false if value already exists
added = settings.AddValueToList("Settings", "IgnoredExtensions", "*.tmp"); // false

// Get all values from list
var extensions = settings.GetList("Settings", "IgnoredExtensions");

// Remove value from list
bool removed = settings.RemoveValueFromList("Settings", "IgnoredExtensions", "*.tmp");

settings.SaveSettings();
```

### Error Handling

```csharp
using wh7r4bb17.AppSettings;

try
{
    var settings = new SettingsManager(@"C:\AppData\settings.xml");

    // This will throw ArgumentException if group or key is null/empty
    settings.Set(null, "Key", "Value");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Argument error: {ex.Message}");
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Null value error: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"File I/O error: {ex.Message}");
}
```

### Working with Multiple Groups

```csharp
using wh7r4bb17.AppSettings;

var settings = new SettingsManager(@"C:\AppData\settings.xml");

// Different groups store independent data
settings.Set("User", "Name", "John Doe");
settings.Set("User", "Email", "john@example.com");

settings.Set("Application", "Theme", "Dark");
settings.Set("Application", "Language", "en-US");

settings.Set("Network", "ProxyAddress", "proxy.company.com");
settings.Set("Network", "ProxyPort", 8080);

// Retrieve from different groups
string userName = settings.Get("User", "Name");
string theme = settings.Get("Application", "Theme");
int proxyPort = settings.GetInteger("Network", "ProxyPort");

settings.SaveSettings();
```

### Persistence and Reload

```csharp
using wh7r4bb17.AppSettings;

var settings = new SettingsManager(@"C:\AppData\settings.xml");

settings.Set("General", "LastRun", DateTime.Now.ToString());
settings.SaveSettings();

// Make some changes
settings.Set("General", "UserName", "NewUser");

// Discard changes and reload from disk
settings.Reload();
string lastRun = settings.Get("General", "LastRun");

settings.Dispose();
```

### Thread-Safe Concurrent Usage

```csharp
using wh7r4bb17.AppSettings;
using System.Threading.Tasks;

var settings = new SettingsManager(@"C:\AppData\settings.xml");

// Multiple threads can safely access the settings
var tasks = new List<Task>();

for (int i = 0; i < 10; i++)
{
    int index = i;
    tasks.Add(Task.Run(() =>
    {
        settings.Set($"Thread{index}", "Value", $"Data{index}");
        var value = settings.Get($"Thread{index}", "Value");
    }));
}

Task.WaitAll(tasks.ToArray());
settings.SaveSettings();
settings.Dispose();
```

### Generated XML Structure

The SettingsManager creates XML files with the following structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Settings>
  <General>
    <ApplicationName>MyApp</ApplicationName>
    <Version>1.0</Version>
    <Debug>True</Debug>
  </General>
  <RecentFiles>
    <Files>C:\file1.txt;C:\file2.txt</Files>
  </RecentFiles>
  <User>
    <Name>John Doe</Name>
    <Email>john@example.com</Email>
  </User>
</Settings>
```

## API Reference

### Constructor

```csharp
public SettingsManager(string fullFilePath, char valueSeparator = ';')
```

### Methods

#### Set Value

```csharp
public void Set(string groupName, string key, object value)
```

#### Get Value

```csharp
public string Get(string groupName, string key, string defaultValue = "")
public bool GetBool(string groupName, string key, bool defaultValue = false)
public int GetInteger(string groupName, string key, int defaultValue = 0)
public double GetDouble(string groupName, string key, double defaultValue = 0.0)
public long GetLong(string groupName, string key, long defaultValue = 0)
public byte GetByte(string groupName, string key, byte defaultValue = 0)
```

#### List Operations

```csharp
public bool AddValueToList(string groupName, string key, string valueToAdd)
public bool RemoveValueFromList(string groupName, string key, string valueToRemove)
public List<string> GetList(string groupName, string key)
```

#### File Operations

```csharp
public void SaveSettings(string xmlRootName = "Settings")
public void Reload()
public void Clear()
```

## Performance Considerations

- The class uses **in-memory caching** - all settings are loaded into memory on instantiation
- **Thread-safe locking** ensures concurrent access safety, but may impact performance under extreme concurrency
- **XML parsing** happens only on `Load()` and `Save()` operations
- Large files with thousands of settings will consume more memory

## Requirements

- .NET Standard 2.0 or compatible runtime
- System.Xml.XmlDocument support

## License

Shield:  [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa], see [license](/license).

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC# About