using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BepInEx.Configuration;

using Cinemachine;

using HarmonyLib;

namespace CalApi.DebugMode;

internal class DebugCameraZoom : IDebug {
    private const string SectionName = "Debug: Camera";

    private readonly DebugMain _main;
    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<float> _zoomAmount;

    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
    public DebugCameraZoom(ConfigFile config, DebugMain main) {
        _main = main;
        _enabled = config.Bind(SectionName, "Camera Zoom", false,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
        _zoomAmount = config.Bind(SectionName, "Camera Zoom Amount", 1f,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
    }

    public void Load() {
        _enabled.SettingChanged += (_, _) => SettingsChanged();
        _zoomAmount.SettingChanged += (_, _) => SettingsChanged();
        SettingsChanged();
    }

    private static readonly FieldInfo followPlayerInfo = AccessTools.Field(typeof(FollowPlayer), "instance");
    private static float _defaultCameraZoom = float.NaN;
    public void SettingsChanged() {
        FollowPlayer followPlayer = (FollowPlayer)followPlayerInfo.GetValue(null);
        if(!followPlayer) return;
        FieldInfo cameraSize = AccessTools.Field(typeof(FollowPlayer), "cameraSize");
        if(float.IsNaN(_defaultCameraZoom)) _defaultCameraZoom = (float)cameraSize.GetValue(followPlayer);
        float zoom = _defaultCameraZoom * (_main.enabled && _enabled.Value ? _zoomAmount.Value :
            (float)_zoomAmount.DefaultValue);

        CinemachineVirtualCamera virtualCamera =
            (CinemachineVirtualCamera)AccessTools.Field(typeof(FollowPlayer), "virtualCamera").GetValue(followPlayer);

        CinemachineVirtualCamera slowMoVirtualCamera =
            (CinemachineVirtualCamera)AccessTools.Field(typeof(FollowPlayer), "slowMoVirtualCamera")
                .GetValue(followPlayer);

        virtualCamera.m_Lens.OrthographicSize = zoom;
        slowMoVirtualCamera.m_Lens.OrthographicSize = zoom;
        // ReSharper disable once HeapView.BoxingAllocation
        cameraSize.SetValue(followPlayer, zoom);
    }

    public void CatControlsAwake() { }
    public void CatControlsInputCheck(Cat.CatControls controls) { }
    public void CatControlsMove(Cat.CatControls controls) { }

    private bool _followPlayerExists;
    private bool _prevFollowPlayerExists;
    public void Update() {
        FollowPlayer followPlayer = (FollowPlayer)followPlayerInfo.GetValue(null);
        _followPlayerExists = followPlayer;
        if(followPlayer && !_prevFollowPlayerExists) SettingsChanged();
        _prevFollowPlayerExists = _followPlayerExists;
    }
}
