using System;
using System.IO;

using BepInEx.Configuration;

namespace CalApi.API;

public static class CustomizationProfiles {
    private const string DefaultProfile = "Default";

    private static ConfigEntry<string>? _customizationProfile;

    private static readonly string profilesPath =
        Path.Combine(Util.moddedStreamingAssetsPath, "Customization Profiles");

    public static string defaultPath { get; } = Path.Combine(profilesPath, DefaultProfile);
    public static string? currentPath { get; private set; }
    public static event EventHandler? profileChanged;

    internal static void LoadSettings(ConfigFile config) {
        _customizationProfile = config.Bind("General", "CustomizationProfile", DefaultProfile,
            $@"The customization profile name to be used by mods for customizing certain aspects of the mod or the game.
Profiles are located in `{profilesPath}`");

        if(!TryUpdateCurrentPath()) currentPath = defaultPath;
        _customizationProfile.SettingChanged += (_, _) => {
            if(!TryUpdateCurrentPath()) return;
            profileChanged?.Invoke(null, EventArgs.Empty);
        };

        Directory.CreateDirectory(defaultPath);
    }

    private static bool TryUpdateCurrentPath() {
        string attemptPath = Path.Combine(profilesPath, _customizationProfile!.Value);
        if(!Directory.Exists(attemptPath)) return false;
        currentPath = attemptPath;
        return true;
    }
}
