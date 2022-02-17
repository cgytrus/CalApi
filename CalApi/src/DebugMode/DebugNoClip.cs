using System.Diagnostics.CodeAnalysis;

using BepInEx.Configuration;

using CalApi.API.Cat;

using Cat;

using UnityEngine;

namespace CalApi.DebugMode;

internal class DebugNoClip : IDebug {
    private const string SectionName = "Debug: No Clip";

    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<float> _noClipSpeed;

    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
    public DebugNoClip(ConfigFile config) {
        _enabled = config.Bind(SectionName, "No Clip", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _noClipSpeed = config.Bind(SectionName, "No Clip Speed", 30f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
    }

    public void Load() { }
    public void SettingsChanged() { }

    private Vector2 _velocity = Vector2.zero;
    public void CatControlsInputCheck(Cat.CatControls controls) {
        if(!_enabled.Value) return;
        bool up = controls.Player.GetButton("Jump");
        bool down = controls.Player.GetButton("Liquid");
        _velocity.y = up == down ? 0f : up ? _noClipSpeed.Value : -_noClipSpeed.Value;
        _velocity.x = controls.Player.GetAxis("Move Horizontal") * _noClipSpeed.Value;
    }

    public void CatControlsMove(Cat.CatControls controls) {
        if(!_enabled.Value) return;
        foreach(Rigidbody2D rb in controls.GetPartManager().GetPartRigidbodies()) {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = _velocity;
        }
    }

    public void CatControlsAwake(CatControls controls) { }
    public void Update() { }
}
