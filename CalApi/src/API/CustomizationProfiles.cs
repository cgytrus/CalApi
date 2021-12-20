using System;
using System.IO;

using BepInEx.Configuration;

namespace CalApi.API;

public static class CustomizationProfiles {
    private const string DefaultProfile = "Default";

    private static ConfigEntry<string>? _customizationProfile;

    private static readonly string profilesPath =
        Path.Combine(Util.moddedStreamingAssetsPath, "Customization Profiles");

    public static readonly string defaultPath = Path.Combine(profilesPath, DefaultProfile);
    public static string? currentPath;
    public static event EventHandler? profileChanged;

    internal static void LoadSettings(ConfigFile config) {
        _customizationProfile = config.Bind("General", "CustomizationProfile", DefaultProfile,
            "The customization profile name to be used by mods for customizing certain aspects of the mod or the game.");

        UpdateCurrentPath();
        _customizationProfile.SettingChanged += (_, _) => {
            UpdateCurrentPath();
            profileChanged?.Invoke(null, EventArgs.Empty);
        };
    }

    private static void UpdateCurrentPath() => currentPath = Path.Combine(profilesPath, _customizationProfile!.Value);
}
