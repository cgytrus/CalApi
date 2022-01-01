using System;
using System.IO;

using BepInEx.Configuration;

// shut up there's no typo
// ReSharper disable once CommentTypo
// you stupid dumbass it's an APIIIIII
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CalApi.API;

public static class CustomizationProfiles {
    private const string DefaultProfile = "Default";
    private const string ReadmeName = "README.txt";

    private static ConfigEntry<string>? _customizationProfile;

    private static readonly string profilesPath =
        Path.Combine(Util.moddedStreamingAssetsPath, "Customization Profiles");

    public static string defaultPath { get; } = Path.Combine(profilesPath, DefaultProfile);
    public static string? currentPath { get; private set; }
    public static event EventHandler? profileChanged;

    internal static void LoadSettings(ConfigFile config) {
        _customizationProfile = config.Bind("General", "CustomizationProfile", DefaultProfile,
            $@"The customization profile name to be used by mods for customizing certain aspects of the mod or the game.
Profiles are located in `{profilesPath}`, read `{ReadmeName}` inside that folder for more info.");

        if(!TryUpdateCurrentPath()) currentPath = defaultPath;
        _customizationProfile.SettingChanged += (_, _) => {
            if(!TryUpdateCurrentPath()) return;
            profileChanged?.Invoke(null, EventArgs.Empty);
        };

        Directory.CreateDirectory(defaultPath);
        CreateReadme(Path.Combine(profilesPath, ReadmeName));
    }

    private static bool TryUpdateCurrentPath() {
        string attemptPath = Path.Combine(profilesPath, _customizationProfile!.Value);
        if(!Directory.Exists(attemptPath)) return false;
        currentPath = attemptPath;
        return true;
    }

    private static void CreateReadme(string path) {
        if(File.Exists(path)) return;
        File.WriteAllText(path, @"This is the folder that contains customization files defined and used by mods.
Check the Default profile folder for mod-specific configuration instructions.");
    }
}
