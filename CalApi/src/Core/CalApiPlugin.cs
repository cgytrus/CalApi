using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;

using CalApi.API;
using CalApi.API.Cat;
using CalApi.Patches;

using HarmonyLib;

using ProphecySystem;

using UnityEngine;

namespace CalApi.Core {
    [BepInPlugin("mod.cgytrus.plugins.calapi", "Cats are Liquid API", "0.2.1")]
    //[BepInProcess("CaL-ABP-Windows.exe")]
    public class CalApiPlugin : BaseUnityPlugin {
        private Vector2 _velocity = Vector2.zero;
        
        private ConfigEntry<bool> _debugMode;
        
        private ConfigEntry<bool> _debugMovement;
        private ConfigEntry<float> _debugNormalMoveSpeed;
        private ConfigEntry<float> _debugLiquidMoveSpeed;
        private ConfigEntry<float> _debugJumpForce;
        private ConfigEntry<float> _debugClimbForce;
        
        private ConfigEntry<bool> _debugNoClip;
        private ConfigEntry<float> _debugNoClipSpeed;
        
        /*private ConfigEntry<bool> _debugCameraZoom;
        private ConfigEntry<float> _debugCameraZoomAmount;*/
        
        private ConfigEntry<bool> _debugInvulnerability;
        private ConfigEntry<bool> _debugFullInvulnerability;
        private ConfigEntry<bool> _debugLavaWalk;
        private ConfigEntry<bool> _debugAlwaysControlled;
        
        private ConfigEntry<bool> _funLiquidJump;

        private readonly HashSet<GameObject> _playerCats = new();
        
        // left overs from the light bulb video thing
        // gonna become a custom items api as a separate mod later
        //public static Sprite lightBulbSprite { get; private set; }
        
        private void Awake() {
            Util.logger = Logger;

            LoadSettings();

            Logger.LogInfo("Applying patches");
            Util.ApplyAllPatches();

            LoadDebugMode();

            Logger.LogInfo("Initializing miscellaneous stuff");

            On.TitleScreen.Awake += (orig, self) => {
                orig(self);
                UI.Initialize();
            };

            //FieldInfo noMetaballsPartTexture = typeof(Cat.CatPartManager).GetField("noMetaballsPartTexture");
            /*On.Cat.CatPartManager.Awake += (orig, self) => {
                if(!self.GetComponent<PlayerActor>()) return;

                lightBulbSprite = (Sprite)noMetaballsPartTexture.GetValue(self);
                //ItemManager.CreateLightBulb(new Vector3(14.5f, -2.5f, 60f));
            };*/

            /*CatPartManager.catShown += (caller, _) => {
                Cat.CatPartManager partManager = (Cat.CatPartManager)caller;
                
                if(!partManager.GetComponent<PlayerActor>()) return;
                
                controls.ApplyColor(Color.black);
            };*/
            
            Logger.LogInfo("Loading complete");
        }
        
        private void LoadSettings() {
            Logger.LogInfo("Loading settings");
            
            _debugMode = Config.Bind("Debug", "Debug Mode", false, "");

            _debugMovement = Config.Bind("Debug: Movement", "Movement", false, "");
            _debugNormalMoveSpeed = Config.Bind("Debug: Movement", "Normal Move Speed", 11.5f, "");
            _debugLiquidMoveSpeed = Config.Bind("Debug: Movement", "Liquid Move Speed", 35f, "");
            _debugJumpForce = Config.Bind("Debug: Movement", "Jump Force", 24f, "");
            _debugClimbForce = Config.Bind("Debug: Movement", "Climb Force", 125f, "");

            _debugNoClip = Config.Bind("Debug: No Clip", "No Clip", false, "");
            _debugNoClipSpeed = Config.Bind("Debug: No Clip", "No Clip Speed", 30f, "");

            _debugInvulnerability = Config.Bind("Debug: Other", "Invulnerability", false, "");
            _debugFullInvulnerability = Config.Bind("Debug: Other", "Full Invulnerability", false, "");
            _debugLavaWalk = Config.Bind("Debug: Other", "Jesus Mode", false, "Walk on lava Pog");
            _debugAlwaysControlled = Config.Bind("Debug: Other", "Always Controlled", false, "");

            _funLiquidJump = Config.Bind("literally fun, nothing more (why did i even add this?)",
                "Jump when liquid", false, "");

            /*_debugCameraZoom = Config.Bind("Debug: Camera Zoom", "Camera Zoom", false, "");
            _debugCameraZoomAmount = Config.Bind("Debug: Camera Zoom", "Camera Zoom Amount", 1f, "");*/
        }
        
        private void LoadDebugMode() {
            Logger.LogInfo("Loading debug mode");
            
            _debugMode.SettingChanged += (_, _) => {
                UpdateDebugMovement();
                UpdateDebugInvulnerability();
                UpdateDebugLavaWalk();
            };
            _debugMovement.SettingChanged += (_, _) => UpdateDebugMovement();
            _debugNormalMoveSpeed.SettingChanged += (_, _) => UpdateDebugMovement();
            _debugLiquidMoveSpeed.SettingChanged += (_, _) => UpdateDebugMovement();
            _debugJumpForce.SettingChanged += (_, _) => UpdateDebugMovement();
            _debugClimbForce.SettingChanged += (_, _) => UpdateDebugMovement();

            _debugMode.SettingChanged += (_, _) => UpdateDebugInvulnerability();
            _debugInvulnerability.SettingChanged += (_, _) => UpdateDebugInvulnerability();
            _debugFullInvulnerability.SettingChanged += (_, _) => UpdateDebugInvulnerability();

            _debugMode.SettingChanged += (_, _) => UpdateDebugLavaWalk();
            _debugLavaWalk.SettingChanged += (_, _) => UpdateDebugLavaWalk();
            UpdateDebugLavaWalk();

            CatControlsJumpWhenLiquidPatch.settingEnabled = _funLiquidJump.Value;
            _funLiquidJump.SettingChanged +=
                (_, _) => CatControlsJumpWhenLiquidPatch.settingEnabled = _funLiquidJump.Value;

            On.Cat.CatControls.Awake += (orig, self) => {
                orig(self);

                _playerCats.Clear();
                if(self.GetComponent<Cat.PlayerActor>()) _playerCats.Add(self.gameObject);

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

                if(_debugMovement.Value) {
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
    }
}
