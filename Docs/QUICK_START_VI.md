# Hướng Dẫn Sử Dụng Nhanh - Remote Config Generator

## Bước 1: Cài Đặt Source Generator

### Thêm vào project (.csproj)

```xml
<ItemGroup>
  <ProjectReference Include="..\SourceGenerator\RemoteConfigGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Bước 2: Tạo Remote Config Class

```csharp
using RemoteConfigGenerator;

[RemoteConfigData(PrefsPrefix = "rc_")]
public static partial class RemoteData
{
    public static int GoldReward = 100;
    public static string WelcomeMessage = "Welcome!";
    public static float SpawnRate = 2.5f;
    public static bool EnableFeatureX = true;
    public static long UserId = 123456789;
}
```

**Lưu ý:**
- Class phải là `static partial`
- Thêm attribute `[RemoteConfigData]`
- Tất cả fields sẽ tự động được quét (không cần `[RemoteConfigField]`)

## Bước 3: Implement Storage

### Option A: Sử dụng PlayerPrefs (đơn giản)

Tạo file `PlayerPrefsStorage.cs`:

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

### Option B: Sử dụng GameData (nâng cao)

Nếu bạn có [game-data-unity](https://github.com/VirtueSky/game-data-unity):

```csharp
using VirtueSky.DataStorage;
using RemoteConfigGenerator;

public class GameDataStorage : IRemoteConfigStorage
{
    public void SetInt(string key, int value) => GameData.Set(key, value);
    public int GetInt(string key, int defaultValue) => GameData.Get(key, defaultValue);
    
    public void SetFloat(string key, float value) => GameData.Set(key, value);
    public float GetFloat(string key, float defaultValue) => GameData.Get(key, defaultValue);
    
    public void SetString(string key, string value) => GameData.Set(key, value);
    public string GetString(string key, string defaultValue) => GameData.Get(key, defaultValue);
    
    public void SetBool(string key, bool value) => GameData.Set(key, value);
    public bool GetBool(string key, bool defaultValue) => GameData.Get(key, defaultValue);
    
    public void SetLong(string key, long value) => GameData.Set(key, value);
    public long GetLong(string key, long defaultValue) => GameData.Get(key, defaultValue);
    
    public void Save() => GameData.Save();
}
```

## Bước 4: Khởi Tạo Storage

Tạo script khởi tạo khi game start:

```csharp
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Assign storage implementation
        RemoteDataExtensions.Storage = new PlayerPrefsStorage();
        // hoặc: RemoteDataExtensions.Storage = new GameDataStorage();
        
        // Load saved data
        RemoteDataExtensions.LoadFromPrefs_Generated();
        
        Debug.Log($"Gold Reward: {RemoteData.GoldReward}");
    }
}
```

## Bước 5: Sử Dụng Trong Game

### Đọc giá trị

```csharp
int gold = RemoteData.GoldReward;
string message = RemoteData.WelcomeMessage;
bool featureEnabled = RemoteData.EnableFeatureX;
```

### Lưu giá trị mới

```csharp
RemoteData.GoldReward = 200;
RemoteData.WelcomeMessage = "Hello World!";

// Lưu vào storage
RemoteDataExtensions.SaveToPrefs_Generated();
```

### Sync từ Firebase Remote Config

```csharp
using Firebase.RemoteConfig;
using System.Threading.Tasks;

public async Task SyncFromFirebase()
{
    await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
    
    // Sync tất cả fields từ Firebase
    RemoteConfig.SyncToStaticClass(RemoteData.SetFieldValue);
    
    // Lưu vào local storage
    RemoteDataExtensions.SaveToPrefs_Generated();
    
    Debug.Log("Synced from Firebase!");
}
```

## Ví Dụ Hoàn Chỉnh

```csharp
using UnityEngine;
using Firebase.RemoteConfig;
using System.Threading.Tasks;

public class RemoteConfigManager : MonoBehaviour
{
    void Awake()
    {
        // 1. Setup storage
        RemoteDataExtensions.Storage = new PlayerPrefsStorage();
        
        // 2. Load từ local
        RemoteDataExtensions.LoadFromPrefs_Generated();
    }

    void Start()
    {
        // 3. Sync từ Firebase (async)
        SyncFromFirebase();
    }

    async Task SyncFromFirebase()
    {
        try
        {
            // Fetch từ Firebase
            await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
            
            // Sync vào static class
            RemoteConfig.SyncToStaticClass(RemoteData.SetFieldValue);
            
            // Lưu vào storage
            RemoteDataExtensions.SaveToPrefs_Generated();
            
            Debug.Log("✅ Remote Config synced!");
            Debug.Log($"Gold Reward: {RemoteData.GoldReward}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Sync failed: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        // Lưu trước khi thoát game
        RemoteDataExtensions.SaveToPrefs_Generated();
    }
}
```

## Debug và Kiểm Tra

### Xem giá trị hiện tại

```csharp
string debugInfo = RemoteDataExtensions.ExportToString_Generated();
Debug.Log(debugInfo);
```

Output:
```
GoldReward: 100
WelcomeMessage: Welcome!
SpawnRate: 2.5
EnableFeatureX: True
UserId: 123456789
```

### Kiểm tra generated code

Sau khi build, tìm file generated trong:
- Visual Studio: `obj/Debug/generated/` 
- Rider: `.generated/`

File: `RemoteData_Generated.g.cs`

## Các Tính Năng Nâng Cao

### 1. Custom Key Names

```csharp
[RemoteConfigData]
public static partial class RemoteData
{
    [RemoteConfigField(Key = "reward_gold")]
    public static int GoldReward = 100;
}
```

### 2. Disable Save/Sync cho field cụ thể

```csharp
[RemoteConfigData]
public static partial class RemoteData
{
    [RemoteConfigField(PersistToPrefs = false)]
    public static int TempValue = 0; // Không lưu vào storage
    
    [RemoteConfigField(SyncFromRemote = false)]
    public static int LocalOnly = 0; // Không sync từ Firebase
}
```

### 3. Custom Prefix

```csharp
[RemoteConfigData(PrefsPrefix = "game_config_")]
public static partial class RemoteData
{
    // Keys sẽ là: game_config_GoldReward, game_config_WelcomeMessage, ...
}
```

### 4. Multiple Config Classes

```csharp
[RemoteConfigData(PrefsPrefix = "game_")]
public static partial class GameConfig
{
    public static int MaxLevel = 100;
}

[RemoteConfigData(PrefsPrefix = "shop_")]
public static partial class ShopConfig
{
    public static int DiamondPrice = 99;
}

// Setup
GameConfigExtensions.Storage = new PlayerPrefsStorage();
ShopConfigExtensions.Storage = new PlayerPrefsStorage();
```

## Troubleshooting

### ❌ Lỗi: "Storage is not set"

**Giải pháp:** Assign storage trước khi gọi Save/Load:
```csharp
RemoteDataExtensions.Storage = new PlayerPrefsStorage();
```

### ❌ Không thấy generated code

**Kiểm tra:**
1. Class có `static partial` chưa?
2. Có attribute `[RemoteConfigData]` chưa?
3. Rebuild project
4. Restart IDE (Visual Studio/Rider)

### ❌ Compile error ở generated code

**Nguyên nhân:** Kiểu dữ liệu không được hỗ trợ

**Supported types:**
- `int`, `float`, `string`, `bool`, `long`
- `int[]`, `float[]`

### ❌ Data không save/load

**Kiểm tra:**
1. Storage đã được assign chưa?
2. Gọi `Save()` sau khi set value
3. Gọi `Load()` trước khi đọc value

## Best Practices

✅ **DO:**
- Assign storage trong `Awake()` hoặc game bootstrap
- Gọi `LoadFromPrefs_Generated()` khi game start
- Gọi `SaveToPrefs_Generated()` trước khi quit game
- Sử dụng async/await cho Firebase sync

❌ **DON'T:**
- Gọi Save() trong Update() (performance issue)
- Quên assign Storage
- Truy cập RemoteData trước khi Load()

## Tài Liệu Tham Khảo

- [STORAGE_WRAPPER_VI.md](STORAGE_WRAPPER_VI.md) - Chi tiết về Storage Wrapper Pattern
- [README_VI.md](README_VI.md) - Hướng dẫn đầy đủ
- [TECHNICAL_DETAILS_VI.md](TECHNICAL_DETAILS_VI.md) - Kỹ thuật nâng cao

## Liên Hệ & Hỗ Trợ

Có vấn đề? Tạo issue trên GitHub hoặc liên hệ maintainer.
