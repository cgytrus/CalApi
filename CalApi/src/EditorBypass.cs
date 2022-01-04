using BepInEx.Configuration;

using Mono.Cecil.Cil;

using MonoMod.Cil;

namespace CalApi;

public class EditorBypass {
    private const string SectionName = "EditorBypass";

    private readonly ConfigEntry<bool> _clippingItemsBypass;
    private readonly ConfigEntry<bool> _itemCostLimitBypass;
    private readonly ConfigEntry<bool> _banListsBypass;
    private readonly ConfigEntry<bool> _maxRoomWorldCountBypass;

    public EditorBypass(ConfigFile config) {
        _clippingItemsBypass = config.Bind(SectionName, "ClippingItemsBypass", false, "");
        _itemCostLimitBypass = config.Bind(SectionName, "ItemCostLimitBypass", false,
        "Required to be enabled for players as well!");
        _banListsBypass = config.Bind(SectionName, "BanListsBypass", false, "");
        _maxRoomWorldCountBypass = config.Bind(SectionName, "MaxRoomWorldCountBypass", false, "");
    }

    public void Load() {
        On.ItemClipCheck.Check += (orig, self) => !_clippingItemsBypass.Value && orig(self);
        On.PolyMap.PolyHelper.GetCostOfItem += (orig, itemId) => _itemCostLimitBypass.Value ? 0 : orig(itemId);
        On.PolyMap.PolyHelper.IsItemInBanList +=
            (orig, banListName, itemId) => !_banListsBypass.Value && orig(banListName, itemId);

        IL.PickerUI.SetMode_Mode += il => {
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
    }
}
