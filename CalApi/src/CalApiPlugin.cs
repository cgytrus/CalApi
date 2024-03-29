﻿using BepInEx;

using CalApi.API;
using CalApi.DebugMode;

namespace CalApi;

[BepInPlugin("mod.cgytrus.plugins.calapi", CalApiPluginInfo.PLUGIN_NAME, CalApiPluginInfo.PLUGIN_VERSION)]
//[BepInProcess("CaL-ABP-Windows.exe")]
internal class CalApiPlugin : BaseUnityPlugin {
    private readonly DebugMain _debugMode;
    private readonly EditorBypass _editorBypass;

    public CalApiPlugin() {
        Util.logger = Logger;

        Logger.LogInfo("Loading settings");

        _debugMode = new DebugMain(Config);
        _editorBypass = new EditorBypass(Config);
        CustomizationProfiles.LoadSettings(Config);
    }

    private void Awake() {
        Logger.LogInfo("Applying patches");
        Util.ApplyAllPatches();

        Logger.LogInfo("Loading debug mode");
        _debugMode.Load();

        Logger.LogInfo("Loading editor bypass");
        _editorBypass.Load();

        Logger.LogInfo("Initializing other stuff");
        UI.Setup();
        UI.AddCopyrightText($"Using {CalApiPluginInfo.PLUGIN_NAME} ({CalApiPluginInfo.PLUGIN_VERSION})");

        Prophecies.Setup(Logger);

        Logger.LogInfo("Loading complete");
    }

    private void Update() => _debugMode.Update();
}
