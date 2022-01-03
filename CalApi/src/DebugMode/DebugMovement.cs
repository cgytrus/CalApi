using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BepInEx.Configuration;

using Cat;

using HarmonyLib;

using UnityEngine;

namespace CalApi.DebugMode;

internal class DebugMovement : IDebug {
    private const string SectionName = "Debug: Movement";

    private readonly DebugMain _main;
    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<float> _normalMoveSpeed;
    private readonly ConfigEntry<float> _liquidMoveSpeed;
    private readonly ConfigEntry<float> _jumpForce;
    private readonly ConfigEntry<float> _climbForce;

    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
    public DebugMovement(ConfigFile config, DebugMain main) {
        _main = main;
        _enabled = config.Bind(SectionName, "Movement", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _normalMoveSpeed = config.Bind(SectionName, "Normal Move Speed", 11.5f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _liquidMoveSpeed = config.Bind(SectionName, "Liquid Move Speed", 35f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _jumpForce = config.Bind(SectionName, "Jump Force", 24f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _climbForce = config.Bind(SectionName, "Climb Force", 125f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
    }

    public void Load() {
        _enabled.SettingChanged += (_, _) => SettingsChanged();
        _normalMoveSpeed.SettingChanged += (_, _) => SettingsChanged();
        _liquidMoveSpeed.SettingChanged += (_, _) => SettingsChanged();
        _jumpForce.SettingChanged += (_, _) => SettingsChanged();
        _climbForce.SettingChanged += (_, _) => SettingsChanged();
    }

    private static readonly FieldInfo normalStateConfiguration =
        AccessTools.Field(typeof(Cat.CatControls), "normalStateConfiguration");
    private static readonly FieldInfo liquidStateConfiguration =
        AccessTools.Field(typeof(Cat.CatControls), "liquidStateConfiguration");
    private static readonly FieldInfo stateConfigurationSpeed =
        AccessTools.Field(normalStateConfiguration.FieldType, "speed");

    private static readonly FieldInfo jumpForce = AccessTools.Field(typeof(Cat.CatControls), "jumpForce");
    private static readonly FieldInfo climbForce = AccessTools.Field(typeof(Cat.CatControls), "climbForce");

    public void SettingsChanged() {
        foreach(GameObject cat in _main.playerCats) {
            Cat.CatControls controls = cat.GetComponent<Cat.CatControls>();

            object normalState = normalStateConfiguration.GetValue(controls);
            object liquidState = liquidStateConfiguration.GetValue(controls);

            if(_main.enabled && _enabled.Value) {
                stateConfigurationSpeed.SetValue(normalState, _normalMoveSpeed.BoxedValue);
                stateConfigurationSpeed.SetValue(liquidState, _liquidMoveSpeed.BoxedValue);
                jumpForce.SetValue(controls, _jumpForce.BoxedValue);
                climbForce.SetValue(controls, _climbForce.BoxedValue);
            }
            else {
                stateConfigurationSpeed.SetValue(normalState, _normalMoveSpeed.DefaultValue);
                stateConfigurationSpeed.SetValue(liquidState, _liquidMoveSpeed.DefaultValue);
                jumpForce.SetValue(controls, _jumpForce.DefaultValue);
                climbForce.SetValue(controls, _climbForce.DefaultValue);
            }

            normalStateConfiguration.SetValue(controls, normalState);
            liquidStateConfiguration.SetValue(controls, liquidState);
        }
    }

    public void CatControlsAwake() => SettingsChanged();
    public void CatControlsInputCheck(CatControls controls) { }
    public void CatControlsMove(CatControls controls) { }
    public void Update() { }
}
