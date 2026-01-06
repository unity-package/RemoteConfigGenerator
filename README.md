# Remote Config Source Generator

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Roslyn](https://img.shields.io/badge/Roslyn-4.3.0-purple.svg)](https://github.com/dotnet/roslyn)

A powerful C# Source Generator for Unity that automatically generates optimized code for Remote Config management with Firebase integration and flexible storage options.

## ✨ Features

- 🚀 **Zero Reflection Overhead** - 50-90% faster than dictionary-based approaches
- 🔒 **Type-Safe** - Compile-time type checking for all config fields
- 🎯 **Auto-Scan** - Automatically detects all public static fields (no manual attribute tagging required)
- 💾 **Flexible Storage** - Pluggable storage with support for PlayerPrefs, GameData, or custom implementations
- 🔌 **Zero Dependencies** - Generated code has no dependencies on specific storage systems
- 🔥 **Firebase Ready** - Built-in support for Firebase Remote Config syncing
- 🛠️ **Easy to Use** - Minimal setup, maximum productivity

## 📋 Table of Contents

- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Building the DLL](#-building-the-dll)
- [Storage Implementation](#-storage-implementation)
- [Usage Examples](#-usage-examples)
- [Advanced Features](#-advanced-features)
- [Documentation](#-documentation)
- [Troubleshooting](#-troubleshooting)

## 📦 Installation

### Thêm vào Unity Project

```
Dependencies: Cài NuGet For Unity => Install Microsoft.CodeAnalysis.CSharp
```

1. Copy file `SourceGenerator.dll` vào thư mục `Assets/Plugins/` trong Unity project
2. Chọn DLL trong Unity Inspector
3. Cấu hình như sau:
   - **Bỏ chọn** "Any Platform"
   - **Chỉ chọn** "Editor"
   - **Thêm Label**: Nhấn vào dropdown "Asset Labels" và thêm label `RoslynAnalyzer`


4. Apply changes

## 🚀 Quick Start

### Step 1: Define Your Config Class

```csharp
using RemoteConfigGenerator;

[RemoteConfigData(PrefsPrefix = "rc_")]
public static partial class RemoteData
{
    // All public static fields are automatically scanned
    public static int GoldReward = 100;
    public static string WelcomeMessage = "Welcome to the game!";
    public static float SpawnRate = 2.5f;
    public static bool EnableNewFeature = false;
    public static long UserId = 123456789;
    
    // Arrays are supported
    public static int[] RewardLevels = { 10, 20, 30, 50, 100 };
    public static float[] Multipliers = { 1.0f, 1.5f, 2.0f };
}
```

**Important:**
- Class must be `static partial`
- Add `[RemoteConfigData]` attribute
- Source Generator automatically scans all public static fields

### Step 2: Implement Storage

Create a storage adapter (see [Storage Implementation](#-storage-implementation) for examples):

```csharp
using UnityEngine;
using RemoteConfigGenerator;

public class PlayerPrefsStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
    
    public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
    
    public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);
    
    public void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    public bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
    
    public void SetLong(string key, long value) => PlayerPrefs.SetString(key, value.ToString());
    public long GetLong(string key, long defaultValue) => 
        long.TryParse(PlayerPrefs.GetString(key, defaultValue.ToString()), out var v) ? v : defaultValue;
    
    public void Save() => PlayerPrefs.Save();
}
```

### Step 3: Initialize and Use

```csharp
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        // 1. Assign storage implementation
        RemoteDataExtensions.Storage = new PlayerPrefsStorage();
        
        // 2. Load saved values
        RemoteDataExtensions.LoadFromPrefs_Generated();
        
        // 3. Use your config
        Debug.Log($"Gold Reward: {RemoteData.GoldReward}");
        Debug.Log($"Welcome: {RemoteData.WelcomeMessage}");
    }
    
    void Start()
    {
        // Modify values
        RemoteData.GoldReward = 200;
        
        // Save changes
        RemoteDataExtensions.SaveToPrefs_Generated();
    }
}
```

## 🔨 Building the DLL

### Prerequisites

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or Rider (optional)

### Using Command Line

```bash
# Navigate to the SourceGenerator directory
cd SourceGenerator

# Build in Debug mode
dotnet build -c Debug

# Build in Release mode (recommended for production)
dotnet build -c Release

# Output location:
# Debug: SourceGenerator/bin/Debug/netstandard2.0/RemoteConfigGenerator.dll
# Release: SourceGenerator/bin/Release/netstandard2.0/RemoteConfigGenerator.dll
```

### Using Visual Studio

1. Open `RemoteConfigGenerator.sln`
2. Right-click on `RemoteConfigGenerator` project
3. Select **Build** or **Rebuild**
4. Find DLL in `SourceGenerator\bin\Debug\netstandard2.0\` or `Release`

### Using Rider

1. Open `RemoteConfigGenerator.sln`
2. Select **Build** → **Build Solution** (Ctrl+Shift+B)
3. Find DLL in `SourceGenerator\bin\Debug\netstandard2.0\` or `Release`

### Build Configurations

| Configuration | Use Case | Optimizations |
|--------------|----------|---------------|
| **Debug** | Development, debugging | No optimizations, includes debug symbols |
| **Release** | Production | Full optimizations, smaller file size |

### Verify Build

```bash
# Check if DLL was created
ls SourceGenerator/bin/Release/netstandard2.0/RemoteConfigGenerator.dll

# View DLL info (Windows)
dotnet --info
```

### Clean Build

```bash
# Clean all build artifacts
dotnet clean

# Clean and rebuild
dotnet clean && dotnet build -c Release
```

## 💾 Storage Implementation

The generator uses a **Storage Wrapper Pattern** - you provide the storage implementation by implementing `IRemoteConfigStorage`.

### Interface Definition

```csharp
namespace RemoteConfigGenerator
{
    public interface IRemoteConfigStorage
    {
        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue);
        
        void SetFloat(string key, float value);
        float GetFloat(string key, float defaultValue);
        
        void SetString(string key, string value);
        string GetString(string key, string defaultValue);
        
        void SetBool(string key, bool value);
        bool GetBool(string key, bool defaultValue);
        
        void SetLong(string key, long value);
        long GetLong(string key, long defaultValue);
        
        void Save();
    }
}
```

### Storage Options

#### 1. PlayerPrefs (Built-in Unity)

Simple, works out of the box:

```csharp
public class PlayerPrefsStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
    // ... implement other methods
    public void Save() => PlayerPrefs.Save();
}
```

**Pros:** No dependencies, works everywhere  
**Cons:** Limited to simple types, slower for large data

#### 2. GameData (game-data-unity package)

High-performance binary serialization:

```csharp
using VirtueSky.DataStorage;

public class GameDataStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) => GameData.Set(key, value);
    public int GetInt(string key, int defaultValue) => GameData.Get(key, defaultValue);
    // ... implement other methods
    public void Save() => GameData.Save();
}
```

**Pros:** Fast, supports complex types, better performance  
**Cons:** Requires [game-data-unity](https://github.com/unity-package/game-data-unity) package

#### 3. Custom Storage

Implement any storage you want:

```csharp
public class CloudStorage : IRemoteConfigStorage
{
    public async void Save()
    {
        // Upload to cloud
        await UploadToCloudAsync();
    }
    // ... implement other methods
}
```

See [STORAGE_WRAPPER_VI.md](Docs/STORAGE_WRAPPER_VI.md) for detailed examples.

## 📚 Usage Examples

### Basic Usage

```csharp
// Read values
int gold = RemoteData.GoldReward;
string msg = RemoteData.WelcomeMessage;
bool enabled = RemoteData.EnableNewFeature;

// Modify values
RemoteData.GoldReward = 500;
RemoteData.EnableNewFeature = true;

// Save to storage
RemoteDataExtensions.SaveToPrefs_Generated();

// Load from storage
RemoteDataExtensions.LoadFromPrefs_Generated();
```

### Firebase Remote Config Integration

```csharp
using Firebase.RemoteConfig;
using System.Threading.Tasks;

public class RemoteConfigManager : MonoBehaviour
{
    async void Start()
    {
        // Initialize storage
        RemoteDataExtensions.Storage = new PlayerPrefsStorage();
        
        // Load local cached values
        RemoteDataExtensions.LoadFromPrefs_Generated();
        
        // Sync from Firebase
        await SyncFromFirebase();
    }
    
    async Task SyncFromFirebase()
    {
        try
        {
            // Fetch latest config from Firebase
            await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
            
            // Sync to static class using generated lookup
            var allKeys = FirebaseRemoteConfig.DefaultInstance.AllKeys;
            foreach (var key in allKeys)
            {
                var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                RemoteDataExtensions.SetFieldValue_Generated(key, configValue);
            }
            
            // Save to local storage
            RemoteDataExtensions.SaveToPrefs_Generated();
            
            Debug.Log("✅ Synced from Firebase");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Sync failed: {ex.Message}");
        }
    }
}
```

### Debug and Export

```csharp
// Export all values to string (useful for debug UI)
string debugInfo = RemoteDataExtensions.ExportToString_Generated();
Debug.Log(debugInfo);

// Output:
// GoldReward: 100
// WelcomeMessage: Welcome to the game!
// SpawnRate: 2.5
// EnableNewFeature: False
```

### Multiple Config Classes

```csharp
[RemoteConfigData(PrefsPrefix = "game_")]
public static partial class GameConfig
{
    public static int MaxLevel = 100;
    public static float DifficultyMultiplier = 1.0f;
}

[RemoteConfigData(PrefsPrefix = "shop_")]
public static partial class ShopConfig
{
    public static int DiamondPrice = 99;
    public static string[] ProductIds = { "com.game.coins", "com.game.gems" };
}

// Setup each independently
GameConfigExtensions.Storage = new PlayerPrefsStorage();
ShopConfigExtensions.Storage = new PlayerPrefsStorage();

GameConfigExtensions.LoadFromPrefs_Generated();
ShopConfigExtensions.LoadFromPrefs_Generated();
```

## 🎯 Advanced Features

### 1. Custom Field Attributes

Control individual field behavior:

```csharp
[RemoteConfigData]
public static partial class RemoteData
{
    // Custom Firebase key name
    [RemoteConfigField(Key = "reward_gold_amount")]
    public static int GoldReward = 100;
    
    // Don't save to storage (runtime only)
    [RemoteConfigField(PersistToPrefs = false)]
    public static int TempValue = 0;
    
    // Don't sync from Firebase (local only)
    [RemoteConfigField(SyncFromRemote = false)]
    public static int LocalOnlyValue = 0;
}
```

### 2. Custom Key Prefix

```csharp
[RemoteConfigData(PrefsPrefix = "myapp_config_")]
public static partial class RemoteData
{
    // Keys will be: myapp_config_GoldReward, myapp_config_WelcomeMessage, etc.
}
```

### 3. Auto-Scan vs Manual Mode

**Auto-Scan (Default):** Automatically includes all public static fields
```csharp
[RemoteConfigData]
public static partial class RemoteData
{
    public static int Field1 = 1;  // ✅ Included
    public static int Field2 = 2;  // ✅ Included
    private static int Field3 = 3; // ❌ Not included (private)
}
```

**Manual Mode:** Only includes fields with `[RemoteConfigField]`
```csharp
[RemoteConfigData]
public static partial class RemoteData
{
    [RemoteConfigField]
    public static int Field1 = 1;  // ✅ Included
    
    public static int Field2 = 2;  // ❌ Not included (no attribute)
}
```

### 4. Supported Types

- ✅ `int`, `float`, `string`, `bool`, `long`
- ✅ `int[]`, `float[]` (stored as comma-separated strings)
- ❌ Complex types (use GameData storage with custom serialization)

## 📖 Documentation

- **[QUICK_START_VI.md](Docs/QUICK_START_VI.md)** - Vietnamese quick start guide
- **[STORAGE_WRAPPER_VI.md](Docs/STORAGE_WRAPPER_VI.md)** - Storage wrapper pattern details
- **[TECHNICAL_DETAILS_VI.md](Docs/TECHNICAL_DETAILS_VI.md)** - Technical implementation details

## 🔧 Troubleshooting

### ❌ "Storage is not set" Error

**Solution:** Assign storage before calling Save/Load:
```csharp
RemoteDataExtensions.Storage = new PlayerPrefsStorage();
```

### ❌ Generated Code Not Appearing

**Check:**
1. ✅ Class is `static partial`
2. ✅ Has `[RemoteConfigData]` attribute
3. ✅ Rebuild project (Clean + Build)
4. ✅ Restart IDE

### ❌ Compile Error in Generated Code

**Cause:** Unsupported field type

**Solution:** Only use supported types (`int`, `float`, `string`, `bool`, `long`, `int[]`, `float[]`)

### ❌ Values Not Saving/Loading

**Check:**
1. ✅ Storage is assigned
2. ✅ Called `SaveToPrefs_Generated()` after modifying values
3. ✅ Called `LoadFromPrefs_Generated()` before reading values

### 🔍 View Generated Code

**Visual Studio:** 
- Solution Explorer → Dependencies → Analyzers → RemoteConfigGenerator → `YourClass_Generated.g.cs`

**File System:**
- `obj/Debug/generated/RemoteConfigGenerator/RemoteConfigGenerator.RemoteConfigSourceGenerator/`

## 🎓 How It Works

```
1. You write:                       2. Source Generator creates:

[RemoteConfigData]                  public static partial class RemoteDataExtensions
public static partial class         {
    RemoteData                          public static IRemoteConfigStorage Storage { get; set; }
{                                       
    public static int Gold = 100;       public static void SaveToPrefs_Generated()
}                                       {
                                            Storage.SetInt("rc_Gold", RemoteData.Gold);
                                            Storage.Save();
                                        }
                                        
                                        public static void LoadFromPrefs_Generated()
                                        {
                                            RemoteData.Gold = Storage.GetInt("rc_Gold", RemoteData.Gold);
                                        }
                                        
                                        // + Dictionary lookups for Firebase
                                        // + Export to string
                                        // + SetFieldValue / GetFieldValue
                                    }
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn) Source Generators
- Inspired by Firebase Remote Config best practices
- Storage wrapper pattern for maximum flexibility

## 📞 Support

- Create an issue on GitHub
- Check the [Documentation](Docs/) folder
- Review [Example](Example/) folder for working code

---

**Made with ❤️ for VirtueSky**
