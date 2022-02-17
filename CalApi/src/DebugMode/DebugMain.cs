using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BepInEx.Configuration;

using Cat;

using UnityEngine;

namespace CalApi.DebugMode;

internal class DebugMain {
    public bool enabled => _enabled.Value;
    public IEnumerable<GameObject> playerCats => _playerCats;

    private readonly ConfigEntry<bool> _enabled;
    private readonly IDebug[] _debugs;
    private readonly HashSet<GameObject> _playerCats = new();

    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
    public DebugMain(ConfigFile config) {
        _enabled = config.Bind("Debug", "Debug Mode", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));

        _debugs = new IDebug[] {
            new DebugMovement(config, this),
            new DebugNoClip(config),
            new DebugCameraZoom(config, this),
            new DebugInvulnerability(config, this),
            new DebugControl(config, this)
        };
    }

    public void Load() {
        _enabled.SettingChanged += (_, _) => {
            foreach(IDebug debug in _debugs) debug.SettingsChanged();
        };
        foreach(IDebug debug in _debugs) debug.Load();

        On.Cat.CatControls.Awake += (orig, self) => {
            orig(self);

            if(self.GetComponent<Cat.PlayerActor>()) {
                _playerCats.Clear();
                _playerCats.Add(self.gameObject);
            }

            CatControlsAwake(self);
        };

        On.Cat.CatControls.InputCheck += (orig, self) => {
            orig(self);
            CatControlsInputCheck(self);
        };

        On.Cat.CatControls.Move += (orig, self) => {
            orig(self);
            CatControlsMove(self);
        };
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void CatControlsAwake(CatControls controls) {
        if(!enabled || !_playerCats.Contains(controls.gameObject)) return;
        foreach(IDebug debug in _debugs) debug.CatControlsAwake(controls);
    }

    private void CatControlsInputCheck(CatControls controls) {
        if(!enabled || !_playerCats.Contains(controls.gameObject)) return;
        foreach(IDebug debug in _debugs) debug.CatControlsInputCheck(controls);
    }

    private void CatControlsMove(CatControls controls) {
        if(!enabled || !_playerCats.Contains(controls.gameObject)) return;
        foreach(IDebug debug in _debugs) debug.CatControlsMove(controls);
    }

    public void Update() {
        foreach(IDebug debug in _debugs) debug.Update();
    }
}
