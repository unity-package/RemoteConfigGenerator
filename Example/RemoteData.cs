/// <summary>
/// Optimized RemoteData using Source Generator
/// 
/// BENEFITS:
/// - Zero reflection overhead (50-90% faster)
/// - Type-safe field access  
/// - Easy to extend - just add [RemoteConfigField] attribute
/// - Automatic PlayerPrefs persistence
/// - Automatic Firebase Remote Config syncing
/// 
/// USAGE:
/// 1. Mark fields with [RemoteConfigField] attribute
/// 2. Source generator automatically creates optimized code
/// 3. Use RemoteDataExtensions.SaveToPrefs_Generated()
/// 4. Use RemoteDataExtensions.LoadFromPrefs_Generated()
/// 5. Use RemoteDataExtensions.FieldSetterLookup dictionary
/// 
/// TO ADD NEW FIELD:
/// Just add [RemoteConfigField] attribute - source generator handles everything!
/// 
/// Example:
///     [RemoteConfigField]
///     public static bool MyNewFeature_Enable = false;
/// 
/// The generator automatically:
/// - Adds to FieldSetterLookup for Firebase sync
/// - Adds to FieldGetterLookup for reading
/// - Adds save/load PlayerPrefs logic
/// - Adds to debug export
/// </summary>

namespace VirtueSky.RemoteConfigGenerated {
    using RemoteConfigGenerator;

    [RemoteConfigData(PrefsPrefix = "rc_")]
    public static partial class RemoteData {
        // ========================================
        // CONTENT & KEYS
        // ========================================

        [RemoteConfigField] public static string ContentReaderKey = "";

        // ========================================
        // VERSIONING & FORCE UPDATE
        // ========================================

        [RemoteConfigField] public static int ForceUpdate = 0;

        [RemoteConfigField] public static int LanguageVersion = 0;

        [RemoteConfigField] public static string LastVersion = "";

        // ========================================
        // SDK & CORE FEATURES
        // ========================================


        [RemoteConfigField] public static bool ActiveLive = true;

        [RemoteConfigField] public static bool ActiveDiamond = false;

        [RemoteConfigField] public static bool ActiveBall3DFake = false;

        [RemoteConfigField] public static bool ActivePreviewSong = false;
        [RemoteConfigField] public static bool ActiveNewShop = false;
    }
}