using System.Collections.Generic;
using System.IO;
using System.Linq;

using BepInEx.Configuration;

using HarmonyLib;

using Mono.Cecil.Cil;

using MonoMod.Cil;

using UnityEngine;
using UnityEngine.UI;

namespace CalApi;

public class EditorBypass {
    private const string SectionName = "EditorBypass";

    private readonly ConfigEntry<bool> _clippingItemsBypass;
    private readonly ConfigEntry<bool> _itemCostLimitBypass;
    private readonly ConfigEntry<bool> _banListsBypass;
    private readonly ConfigEntry<bool> _maxRoomWorldCountBypass;
    private readonly ConfigEntry<bool> _nameLengthLimitBypass;
    private readonly ConfigEntry<bool> _verificationBypass;

    public EditorBypass(ConfigFile config) {
        _clippingItemsBypass = config.Bind(SectionName, "ClippingItemsBypass", false, "");
        _itemCostLimitBypass = config.Bind(SectionName, "ItemCostLimitBypass", false,
        "Required to be enabled for players as well!");
        _banListsBypass = config.Bind(SectionName, "BanListsBypass", false, "");
        _maxRoomWorldCountBypass = config.Bind(SectionName, "MaxRoomWorldCountBypass", false, "");
        _nameLengthLimitBypass = config.Bind(SectionName, "NameLengthLimitBypass", false, "");
        _verificationBypass = config.Bind(SectionName, "VerificationBypass", false, "");
    }

    public void Load() {
        ClippingItemsBypass();
        ItemCostLimitBypass();
        BanListsBypass();
        MaxWorldRoomCountBypass();
        NameLengthLimitBypass();
        VerificationBypass();
    }

    private void ClippingItemsBypass() => On.ItemClipCheck.Check += (orig, self) => !_clippingItemsBypass.Value && orig(self);

    private void ItemCostLimitBypass() => On.PolyMap.PolyHelper.GetCostOfItem += (orig, itemId) => _itemCostLimitBypass.Value ? 0 : orig(itemId);

    private void BanListsBypass() => On.PolyMap.PolyHelper.IsItemInBanList +=
        (orig, banListName, itemId) => !_banListsBypass.Value && orig(banListName, itemId);

    private void MaxWorldRoomCountBypass() => IL.PickerUI.SetMode_Mode += il => {
        ILCursor cursor = new(il);

        for(int i = 0; i < 2; i++) {
            int localIndex = 3 + i;
            cursor.GotoNext(code => code.MatchStloc(localIndex));
            cursor.EmitReference(_maxRoomWorldCountBypass);
            cursor.Emit<ConfigEntry<bool>>(OpCodes.Call, $"get_{nameof(ConfigEntry<bool>.Value)}");
            cursor.Emit(OpCodes.Not);
            cursor.Emit(OpCodes.And);
        }
    };

    private void NameLengthLimitBypass() {
        int vanillaCharacterLimit = -1;
        InputField? lastNewName = null;
        On.CreateUI.Awake += (orig, self) => {
            orig(self);
            lastNewName = (InputField)AccessTools.Field(typeof(CreateUI), "newName").GetValue(self);
            if(vanillaCharacterLimit < 0) vanillaCharacterLimit = lastNewName.characterLimit;
            lastNewName.characterLimit = _nameLengthLimitBypass.Value ? 0 : vanillaCharacterLimit;
        };

        _nameLengthLimitBypass.SettingChanged += (_, _) => {
            if(lastNewName && vanillaCharacterLimit >= 0)
                lastNewName!.characterLimit = _nameLengthLimitBypass.Value ? 0 : vanillaCharacterLimit;
        };
    }

    private void VerificationBypass() {
        On.RoomEditorSharePlaytest.StartPlaytest += orig => {
            if(!_verificationBypass.Value) {
                orig();
                return;
            }

            AccessTools.Property(typeof(RoomEditorSharePlaytest), nameof(RoomEditorSharePlaytest.PlaytestActive))
                .SetValue(null, true);
            Door.CommunityPackCompletedAction?.Invoke();
        };

        IL.RoomEditorSharePlaytest.SetProgressionArrays += il => {
            ILCursor cursor = new(il);
            cursor.GotoNext(code => code.MatchStloc(3));
            cursor.Index -= 2;
            cursor.EmitReference(this);
            cursor.Index++;
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit<EditorBypass>(OpCodes.Call, nameof(GeneratePackProgression));
        };
    }

    private Dictionary<string, List<string>> GeneratePackProgression(
        Dictionary<string, List<string>> progressionGuids, string packPath) {
        if(!_verificationBypass.Value) return progressionGuids;
        progressionGuids.Clear();

        string[] packProgression = ProgressionArrayGenerator.GetPackProgressionArray(packPath, false);
        Dictionary<string, List<string>> worldProgressions = new(packProgression.Length);

        foreach(string worldPath in Directory.EnumerateDirectories(packPath)) {
            string worldSettingsPath = Path.Combine(worldPath, "settings.data");
            if(!File.Exists(worldSettingsPath)) continue;

            WorldSettings.Settings worldSettings =
                JsonUtility.FromJson<WorldSettings.Settings>(File.ReadAllText(worldSettingsPath));

            string[] worldProgression = ProgressionArrayGenerator.GetWorldProgressionArray(worldPath);
            worldProgressions.Add(worldSettings.worldGUID, worldProgression.ToList());
        }

        foreach(string worldGuid in packProgression) progressionGuids.Add(worldGuid, worldProgressions[worldGuid]);
        return progressionGuids;
    }
}
