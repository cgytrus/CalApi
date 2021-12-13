using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;

using CalApi.API;
using CalApi.API.Cat;
using CalApi.Patches;

using Cinemachine;

using HarmonyLib;

using ProphecySystem;

using UnityEngine;

namespace CalApi;

[BepInPlugin("mod.cgytrus.plugins.calapi", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
//[BepInProcess("CaL-ABP-Windows.exe")]
public class CalApiPlugin : BaseUnityPlugin {
    private Vector2 _velocity = Vector2.zero;

    private readonly ConfigEntry<bool> _debugMode;

    private readonly ConfigEntry<bool> _debugMovement;
    private readonly ConfigEntry<float> _debugNormalMoveSpeed;
    private readonly ConfigEntry<float> _debugLiquidMoveSpeed;
    private readonly ConfigEntry<float> _debugJumpForce;
    private readonly ConfigEntry<float> _debugClimbForce;

    private readonly ConfigEntry<bool> _debugNoClip;
    private readonly ConfigEntry<float> _debugNoClipSpeed;

    private readonly ConfigEntry<bool> _debugCameraZoom;
    private readonly ConfigEntry<float> _debugCameraZoomAmount;

    private readonly ConfigEntry<bool> _debugInvulnerability;
    private readonly ConfigEntry<bool> _debugFullInvulnerability;
    private readonly ConfigEntry<bool> _debugLavaWalk;
    private readonly ConfigEntry<bool> _debugAlwaysControlled;
    private readonly ConfigEntry<bool> _debugLiquidJump;

    private readonly HashSet<GameObject> _playerCats = new();

    public CalApiPlugin() {
        Util.logger = Logger;

        Logger.LogInfo("Loading settings");

        _debugMode = Config.Bind("Debug", "Debug Mode", false, "");

        _debugMovement = Config.Bind("Debug: Movement", "Movement", false, "");
        _debugNormalMoveSpeed = Config.Bind("Debug: Movement", "Normal Move Speed", 11.5f, "");
        _debugLiquidMoveSpeed = Config.Bind("Debug: Movement", "Liquid Move Speed", 35f, "");
        _debugJumpForce = Config.Bind("Debug: Movement", "Jump Force", 24f, "");
        _debugClimbForce = Config.Bind("Debug: Movement", "Climb Force", 125f, "");

        _debugNoClip = Config.Bind("Debug: No Clip", "No Clip", false, "");
        _debugNoClipSpeed = Config.Bind("Debug: No Clip", "No Clip Speed", 30f, "");

        _debugCameraZoom = Config.Bind("Debug: Camera", "Camera Zoom", false, "");
        _debugCameraZoomAmount = Config.Bind("Debug: Camera", "Camera Zoom Amount", 1f, "");

        _debugInvulnerability = Config.Bind("Debug: Other", "Invulnerability", false, "");
        _debugFullInvulnerability = Config.Bind("Debug: Other", "Full Invulnerability", false, "");
        _debugLavaWalk = Config.Bind("Debug: Other", "Jesus Mode", false, "Walk on lava Pog");
        _debugAlwaysControlled = Config.Bind("Debug: Other", "Always Controlled", false, "");
        _debugLiquidJump = Config.Bind("Debug: Other", "Jump when liquid", false, "");
    }

    private void Awake() {
        Logger.LogInfo("Applying patches");
        Util.ApplyAllPatches();

        LoadDebugMode();

        Logger.LogInfo("Initializing other stuff");
        UI.Setup();
        UI.AddCopyrightText($"Using {PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_VERSION})");

        Logger.LogInfo("Loading complete");
    }

    private void LoadDebugMode() {
        Logger.LogInfo("Loading debug mode");

        _debugMode.SettingChanged += (_, _) => {
            UpdateDebugMovement();
            UpdateDebugInvulnerability();
            UpdateDebugLavaWalk();
            UpdateDebugJumpWhenLiquid();
            UpdateDebugCameraZoom();
        };
        _debugMovement.SettingChanged += (_, _) => UpdateDebugMovement();
        _debugNormalMoveSpeed.SettingChanged += (_, _) => UpdateDebugMovement();
        _debugLiquidMoveSpeed.SettingChanged += (_, _) => UpdateDebugMovement();
        _debugJumpForce.SettingChanged += (_, _) => UpdateDebugMovement();
        _debugClimbForce.SettingChanged += (_, _) => UpdateDebugMovement();

        _debugInvulnerability.SettingChanged += (_, _) => UpdateDebugInvulnerability();
        _debugFullInvulnerability.SettingChanged += (_, _) => UpdateDebugInvulnerability();

        _debugLavaWalk.SettingChanged += (_, _) => UpdateDebugLavaWalk();
        UpdateDebugLavaWalk();

        _debugLiquidJump.SettingChanged += (_, _) => UpdateDebugJumpWhenLiquid();
        UpdateDebugJumpWhenLiquid();

        On.Cat.CatControls.Awake += (orig, self) => {
            orig(self);

            if(self.GetComponent<Cat.PlayerActor>()) {
                _playerCats.Clear();
                _playerCats.Add(self.gameObject);
            }

            UpdateDebugMovement();
            UpdateDebugInvulnerability();
        };

        On.Cat.CatControls.InputCheck += (orig, self) => {
            orig(self);

            if(!_debugMode.Value || !_playerCats.Contains(self.gameObject)) return;
            ProcessAlwaysControlled(self);
            ProcessNoClip(self);
        };

        On.Cat.CatControls.Move += (orig, self) => {
            orig(self);

            if(!_debugMode.Value || !_debugNoClip.Value || !_playerCats.Contains(self.gameObject)) return;
            ProcessNoClipToggle(self);
        };

        _debugCameraZoom.SettingChanged += (_, _) => UpdateDebugCameraZoom();
        _debugCameraZoomAmount.SettingChanged += (_, _) => UpdateDebugCameraZoom();
        UpdateDebugCameraZoom();
    }

    private void ProcessAlwaysControlled(Cat.CatControls controls) {
        if(!_debugAlwaysControlled.Value) return;
        controls.PlayerControlled = true;
        controls.AllowControl = true;
        controls.AllowMovement = true;
        Prophet.ActiveProphecyProhibitsControl = 0;
    }

    private void ProcessNoClip(Cat.CatControls controls) {
        if(!_debugNoClip.Value) return;
        bool up = controls.Player.GetButton("Jump");
        bool down = controls.Player.GetButton("Liquid");
        _velocity.y = up == down ? 0f : up ? _debugNoClipSpeed.Value : -_debugNoClipSpeed.Value;
        _velocity.x = controls.Player.GetAxis("Move Horizontal") * _debugNoClipSpeed.Value;
    }

    private void ProcessNoClipToggle(Cat.CatControls controls) {
        foreach(Rigidbody2D rb in controls.GetPartManager().GetPartRigidbodies()) {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = _velocity;
        }
    }

    private static readonly FieldInfo normalStateConfiguration =
        AccessTools.Field(typeof(Cat.CatControls), "normalStateConfiguration");
    private static readonly FieldInfo liquidStateConfiguration =
        AccessTools.Field(typeof(Cat.CatControls), "liquidStateConfiguration");
    private static readonly FieldInfo stateConfigurationSpeed =
        AccessTools.Field(normalStateConfiguration.FieldType, "speed");

    private static readonly FieldInfo jumpForce = AccessTools.Field(typeof(Cat.CatControls), "jumpForce");
    private static readonly FieldInfo climbForce = AccessTools.Field(typeof(Cat.CatControls), "climbForce");

    private void UpdateDebugMovement() {
        foreach(GameObject cat in _playerCats) {
            Cat.CatControls controls = cat.GetComponent<Cat.CatControls>();

            object normalState = normalStateConfiguration.GetValue(controls);
            object liquidState = liquidStateConfiguration.GetValue(controls);

            if(_debugMode.Value && _debugMovement.Value) {
                stateConfigurationSpeed.SetValue(normalState, _debugNormalMoveSpeed.BoxedValue);
                stateConfigurationSpeed.SetValue(liquidState, _debugLiquidMoveSpeed.BoxedValue);
                jumpForce.SetValue(controls, _debugJumpForce.BoxedValue);
                climbForce.SetValue(controls, _debugClimbForce.BoxedValue);
            }
            else {
                stateConfigurationSpeed.SetValue(normalState, _debugNormalMoveSpeed.DefaultValue);
                stateConfigurationSpeed.SetValue(liquidState, _debugLiquidMoveSpeed.DefaultValue);
                jumpForce.SetValue(controls, _debugJumpForce.DefaultValue);
                climbForce.SetValue(controls, _debugClimbForce.DefaultValue);
            }

            normalStateConfiguration.SetValue(controls, normalState);
            liquidStateConfiguration.SetValue(controls, liquidState);
        }
    }

    private static readonly FieldInfo catHealth = AccessTools.Field(typeof(Cat.CatControls), "catHealth");
    private static readonly FieldInfo invulnerable = AccessTools.Field(catHealth.FieldType, "invulnerable");
    private static readonly FieldInfo dying = AccessTools.Field(catHealth.FieldType, "dying");
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    private void UpdateDebugInvulnerability() {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach(GameObject cat in _playerCats) {
            Cat.CatControls controls = cat.GetComponent<Cat.CatControls>();
            object health = catHealth.GetValue(controls);
            invulnerable.SetValue(health, _debugMode.Value && _debugInvulnerability.Value);
            dying.SetValue(health, _debugMode.Value && _debugFullInvulnerability.Value);
        }
    }

    private void UpdateDebugLavaWalk() {
        int companion = LayerMask.NameToLayer("Companion Collider");
        bool ignore = !_debugMode.Value || !_debugLavaWalk.Value;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Cat"), companion, ignore);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Liquid Cat"), companion, ignore);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Floating Cat"), companion, ignore);
    }

    private void UpdateDebugJumpWhenLiquid() =>
        CatControlsJumpWhenLiquidPatch.enabled = _debugMode.Value && _debugLiquidJump.Value;

    private static readonly FieldInfo followPlayerInfo = AccessTools.Field(typeof(FollowPlayer), "instance");
    private static float _defaultCameraZoom = float.NaN;
    private void UpdateDebugCameraZoom() {
        FollowPlayer followPlayer = (FollowPlayer)followPlayerInfo.GetValue(null);
        if(!followPlayer) return;
        FieldInfo cameraSize = AccessTools.Field(typeof(FollowPlayer), "cameraSize");
        if(float.IsNaN(_defaultCameraZoom)) _defaultCameraZoom = (float)cameraSize.GetValue(followPlayer);
        float zoom = _defaultCameraZoom * (_debugMode.Value && _debugCameraZoom.Value ? _debugCameraZoomAmount.Value :
            (float)_debugCameraZoomAmount.DefaultValue);

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

    private void Update() => CheckDebugCameraZoomFollowPlayer();

    private bool _followPlayerExists;
    private bool _prevFollowPlayerExists;
    private void CheckDebugCameraZoomFollowPlayer() {
        FollowPlayer followPlayer = (FollowPlayer)followPlayerInfo.GetValue(null);
        _followPlayerExists = followPlayer;
        if(followPlayer && !_prevFollowPlayerExists) UpdateDebugCameraZoom();
        _prevFollowPlayerExists = _followPlayerExists;
    }
}
