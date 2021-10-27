using System;
using System.Linq;

using BepInEx.Configuration;

namespace CalApi.Patches {
    public abstract class ConfigurablePatch : IPatch {
        protected static bool enabled { get; private set; }

        protected ConfigurablePatch(ConfigFile config, string section, string key, bool defaultValue,
            string description) {
            if(string.IsNullOrWhiteSpace(key)) key = GetType().FullName;
            if(config.Select(conf => conf.Key.Key).Contains(key))
                throw new InvalidOperationException("Tried loading a fix which is already loaded.");

            ConfigEntry<bool> configEntry = config.Bind(section, key, defaultValue, description);
            enabled = configEntry.Value;
            configEntry.SettingChanged += (_, _) => { enabled = configEntry.Value; };
        }

        public abstract void Apply();
    }
}
