using BepInEx;

using CalApi.API;
using CalApi.DebugMode;

namespace CalApi;

[BepInPlugin("mod.cgytrus.plugins.calapi", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
//[BepInProcess("CaL-ABP-Windows.exe")]
internal class CalApiPlugin : BaseUnityPlugin {
    private readonly DebugMain _debugMode;

    public CalApiPlugin() {
        Util.logger = Logger;

        Logger.LogInfo("Loading settings");

        _debugMode = new DebugMain(Config);

        CustomizationProfiles.LoadSettings(Config);
    }

    private void Awake() {
        Logger.LogInfo("Applying patches");
        Util.ApplyAllPatches();

        Logger.LogInfo("Loading debug mode");
        _debugMode.Load();

        Logger.LogInfo("Initializing other stuff");
        UI.Setup();
        UI.AddCopyrightText($"Using {PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_VERSION})");

        Logger.LogInfo("Loading complete");
    }

    private void Update() => _debugMode.Update();
}
