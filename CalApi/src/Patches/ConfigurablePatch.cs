using System;
using System.Linq;

using BepInEx.Configuration;

using JetBrains.Annotations;

namespace CalApi.Patches;

[UsedImplicitly]
public abstract class ConfigurablePatch : IPatch {
    [UsedImplicitly]
    protected bool enabled { get; private set; }

    protected ConfigurablePatch(ConfigFile config, string section, string? key, bool defaultValue,
        string? description) {
        if(string.IsNullOrWhiteSpace(key)) key = GetType().FullName;
        if(config.Select(conf => conf.Key).Any(def => def.Section == section && def.Key == key))
            throw new InvalidOperationException("Tried loading a patch which is already loaded.");

        ConfigEntry<bool> configEntry = config.Bind(section, key, defaultValue, description ?? "");
        enabled = configEntry.Value;
        configEntry.SettingChanged += (_, _) => { enabled = configEntry.Value; };
    }

    public abstract void Apply();
}
