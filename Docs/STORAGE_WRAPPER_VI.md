# Hướng Dẫn Sử Dụng Storage Wrapper

## Tổng Quan

Source Generator sử dụng **Storage Wrapper Pattern** để tách biệt logic save/load khỏi implementation cụ thể. Điều này giúp:
- ✅ **Zero dependencies** - Generated code không phụ thuộc vào PlayerPrefs hay GameData
- ✅ **Linh hoạt** - Dễ dàng đổi storage implementation
- ✅ **Testable** - Có thể mock storage cho unit test
- ✅ **Custom storage** - Implement bất kỳ storage nào (file, cloud, database...)

## Cách Hoạt Động

### 1. Interface được generate tự động

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

### 2. Generated code gọi qua wrapper

```csharp
public static partial class RemoteDataExtensions
{
    // Property để inject storage
    public static IRemoteConfigStorage Storage { get; set; }

    public static void SaveToPrefs_Generated()
    {
        if (Storage == null) 
        {
            Debug.LogError("Storage is not set!");
            return;
        }
        
        // Gọi qua interface
        Storage.SetInt("rc_GoldReward", RemoteData.GoldReward);
        Storage.SetString("rc_WelcomeMessage", RemoteData.WelcomeMessage);
        Storage.Save();
    }
}
```

## Hướng Dẫn Implementation

### Option 1: Sử Dụng PlayerPrefs

```csharp
using UnityEngine;
using RemoteConfigGenerator;

public class PlayerPrefsStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) 
        => PlayerPrefs.SetInt(key, value);
    
    public int GetInt(string key, int defaultValue) 
        => PlayerPrefs.GetInt(key, defaultValue);
    
    public void SetFloat(string key, float value) 
        => PlayerPrefs.SetFloat(key, value);
    
    public float GetFloat(string key, float defaultValue) 
        => PlayerPrefs.GetFloat(key, defaultValue);
    
    public void SetString(string key, string value) 
        => PlayerPrefs.SetString(key, value);
    
    public string GetString(string key, string defaultValue) 
        => PlayerPrefs.GetString(key, defaultValue);
    
    public void SetBool(string key, bool value) 
        => PlayerPrefs.SetInt(key, value ? 1 : 0);
    
    public bool GetBool(string key, bool defaultValue) 
        => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
    
    public void SetLong(string key, long value) 
        => PlayerPrefs.SetString(key, value.ToString());
    
    public long GetLong(string key, long defaultValue)
    {
        var str = PlayerPrefs.GetString(key, defaultValue.ToString());
        return long.TryParse(str, out var val) ? val : defaultValue;
    }
    
    public void Save() 
        => PlayerPrefs.Save();
}
```

### Option 2: Sử Dụng GameData (game-data-unity)

```csharp
using VirtueSky.DataStorage;
using RemoteConfigGenerator;

public class GameDataStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) 
        => GameData.Set(key, value);
    
    public int GetInt(string key, int defaultValue) 
        => GameData.Get(key, defaultValue);
    
    public void SetFloat(string key, float value) 
        => GameData.Set(key, value);
    
    public float GetFloat(string key, float defaultValue) 
        => GameData.Get(key, defaultValue);
    
    public void SetString(string key, string value) 
        => GameData.Set(key, value);
    
    public string GetString(string key, string defaultValue) 
        => GameData.Get(key, defaultValue);
    
    public void SetBool(string key, bool value) 
        => GameData.Set(key, value);
    
    public bool GetBool(string key, bool defaultValue) 
        => GameData.Get(key, defaultValue);
    
    public void SetLong(string key, long value) 
        => GameData.Set(key, value);
    
    public long GetLong(string key, long defaultValue) 
        => GameData.Get(key, defaultValue);
    
    public void Save() 
        => GameData.Save();
}
```

### Option 3: Custom Storage (File System)

```csharp
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using RemoteConfigGenerator;

public class FileStorage : IRemoteConfigStorage
{
    private Dictionary<string, string> data = new Dictionary<string, string>();
    private string filePath;

    public FileStorage(string fileName = "remoteconfig.dat")
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        Load();
    }

    private void Load()
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SerializableDictionary>(json)?.ToDictionary() 
                   ?? new Dictionary<string, string>();
        }
    }

    public void SetInt(string key, int value) 
        => data[key] = value.ToString();
    
    public int GetInt(string key, int defaultValue)
        => data.TryGetValue(key, out var val) && int.TryParse(val, out var result) 
           ? result : defaultValue;
    
    public void SetFloat(string key, float value) 
        => data[key] = value.ToString();
    
    public float GetFloat(string key, float defaultValue)
        => data.TryGetValue(key, out var val) && float.TryParse(val, out var result) 
           ? result : defaultValue;
    
    public void SetString(string key, string value) 
        => data[key] = value;
    
    public string GetString(string key, string defaultValue)
        => data.TryGetValue(key, out var val) ? val : defaultValue;
    
    public void SetBool(string key, bool value) 
        => data[key] = value.ToString();
    
    public bool GetBool(string key, bool defaultValue)
        => data.TryGetValue(key, out var val) && bool.TryParse(val, out var result) 
           ? result : defaultValue;
    
    public void SetLong(string key, long value) 
        => data[key] = value.ToString();
    
    public long GetLong(string key, long defaultValue)
        => data.TryGetValue(key, out var val) && long.TryParse(val, out var result) 
           ? result : defaultValue;
    
    public void Save()
    {
        var json = JsonUtility.ToJson(new SerializableDictionary(data));
        File.WriteAllText(filePath, json);
    }

    [System.Serializable]
    private class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializableDictionary() { }
        
        public SerializableDictionary(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }
}
```

## Cách Sử Dụng

### 1. Khởi tạo storage khi game start

```csharp
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        // Chọn storage implementation
        RemoteDataExtensions.Storage = new PlayerPrefsStorage();
        // hoặc
        // RemoteDataExtensions.Storage = new GameDataStorage();
        // hoặc
        // RemoteDataExtensions.Storage = new FileStorage();
    }
}
```

### 2. Sử dụng save/load như bình thường

```csharp
// Load data từ storage
RemoteDataExtensions.LoadFromPrefs_Generated();

// Sử dụng data
Debug.Log(RemoteData.GoldReward);
Debug.Log(RemoteData.WelcomeMessage);

// Thay đổi và save
RemoteData.GoldReward = 200;
RemoteDataExtensions.SaveToPrefs_Generated();
```

### 3. Đổi storage runtime (nếu cần)

```csharp
// Đổi từ PlayerPrefs sang GameData
RemoteDataExtensions.Storage = new GameDataStorage();
RemoteDataExtensions.LoadFromPrefs_Generated();
```

## Unit Testing

```csharp
using NUnit.Framework;
using RemoteConfigGenerator;

// Mock storage cho testing
public class MockStorage : IRemoteConfigStorage
{
    private Dictionary<string, object> storage = new Dictionary<string, object>();

    public void SetInt(string key, int value) => storage[key] = value;
    public int GetInt(string key, int defaultValue) 
        => storage.TryGetValue(key, out var val) ? (int)val : defaultValue;
    
    // ... implement các method khác tương tự
    
    public void Save() { /* do nothing */ }
}

public class RemoteDataTests
{
    [SetUp]
    public void Setup()
    {
        RemoteDataExtensions.Storage = new MockStorage();
    }

    [Test]
    public void TestSaveLoad()
    {
        RemoteData.GoldReward = 100;
        RemoteDataExtensions.SaveToPrefs_Generated();
        
        RemoteData.GoldReward = 0;
        RemoteDataExtensions.LoadFromPrefs_Generated();
        
        Assert.AreEqual(100, RemoteData.GoldReward);
    }
}
```

## Best Practices

### 1. Singleton Pattern cho Storage

```csharp
public class StorageManager
{
    private static IRemoteConfigStorage _instance;
    
    public static IRemoteConfigStorage Instance
    {
        get
        {
            if (_instance == null)
            {
                // Auto-initialize với default storage
                _instance = new PlayerPrefsStorage();
            }
            return _instance;
        }
        set => _instance = value;
    }
}

// Sử dụng
RemoteDataExtensions.Storage = StorageManager.Instance;
```

### 2. Auto-assign trong Extension class

```csharp
// Tạo partial class để extend
public static partial class RemoteDataExtensions
{
    static RemoteDataExtensions()
    {
        // Auto-assign storage khi class được load
        if (Storage == null)
        {
            Storage = new PlayerPrefsStorage();
        }
    }
}
```

### 3. Logging và Error Handling

```csharp
public class LoggingStorage : IRemoteConfigStorage
{
    private IRemoteConfigStorage inner;

    public LoggingStorage(IRemoteConfigStorage storage)
    {
        inner = storage;
    }

    public void SetInt(string key, int value)
    {
        Debug.Log($"[Storage] SetInt: {key} = {value}");
        inner.SetInt(key, value);
    }
    
    // Wrap tất cả các method khác...
}

// Sử dụng
RemoteDataExtensions.Storage = new LoggingStorage(new PlayerPrefsStorage());
```

## FAQ

**Q: Phải implement storage không?**  
A: Có, bạn phải implement và assign `IRemoteConfigStorage` trước khi gọi Save/Load. Nếu không sẽ có error log.

**Q: Có thể đổi storage runtime không?**  
A: Có, chỉ cần assign lại `RemoteDataExtensions.Storage = new OtherStorage()`.

**Q: Arrays (int[], float[]) được xử lý như thế nào?**  
A: Arrays được serialize thành string dạng comma-separated (`"1,2,3"`) và lưu qua `SetString()`.

**Q: Có thể dùng nhiều storage cùng lúc không?**  
A: Có, bạn có thể implement composite storage:
```csharp
public class DualStorage : IRemoteConfigStorage
{
    private IRemoteConfigStorage primary;
    private IRemoteConfigStorage backup;
    
    public void SetInt(string key, int value)
    {
        primary.SetInt(key, value);
        backup.SetInt(key, value);
    }
    // ...
}
```

**Q: Performance có ảnh hưởng không?**  
A: Interface call có overhead rất nhỏ (~1-2ns). Hầu hết performance phụ thuộc vào storage implementation (PlayerPrefs, GameData, File I/O).

## Tham Khảo

- [README_VI.md](README_VI.md) - Hướng dẫn cơ bản
- [TECHNICAL_DETAILS_VI.md](TECHNICAL_DETAILS_VI.md) - Chi tiết kỹ thuật
- [game-data-unity](https://github.com/VirtueSky/game-data-unity) - GameData package
