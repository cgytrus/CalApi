using System.Diagnostics.CodeAnalysis;

using BepInEx.Configuration;

using CalApi.Patches;

using Cat;

using ProphecySystem;

namespace CalApi.DebugMode;

internal class DebugControl : IDebug {
    private const string SectionName = "Debug: Control";

    private readonly DebugMain _main;
    private readonly ConfigEntry<bool> _alwaysControlled;
    private readonly ConfigEntry<bool> _liquidJump;

    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
    public DebugControl(ConfigFile config, DebugMain main) {
        _main = main;
        _alwaysControlled = config.Bind(SectionName, "Always Controlled", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _liquidJump = config.Bind(SectionName, "Jump when liquid", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
    }

    public void Load() {
        _liquidJump.SettingChanged += (_, _) => SettingsChanged();
        SettingsChanged();
    }

    public void SettingsChanged() => CatControlsJumpWhenLiquidPatch.enabled = _main.enabled && _liquidJump.Value;

    public void CatControlsInputCheck(Cat.CatControls controls) {
        if(!_alwaysControlled.Value) return;
        controls.PlayerControlled = true;
        controls.AllowControl = true;
        controls.AllowMovement = true;
        Prophet.ActiveProphecyProhibitsControl = 0;
    }

    public void CatControlsAwake(CatControls controls) { }
    public void CatControlsMove(Cat.CatControls controls) { }
    public void Update() { }
}
