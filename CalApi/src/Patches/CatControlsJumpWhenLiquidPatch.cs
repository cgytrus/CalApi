using Cat;

using Mono.Cecil.Cil;

using MonoMod.Cil;

namespace CalApi.Patches;

/*[HarmonyPatch(typeof(CatControls))]
[HarmonyPatch("Jump")]*/
// ReSharper disable once ClassNeverInstantiated.Global
internal class CatControlsJumpWhenLiquidPatch : IPatch {
    public static bool settingEnabled;

    public void Apply() => IL.Cat.CatControls.Jump += il => {
        ILCursor cursor = new(il);
        cursor.GotoNext(MoveType.After, code => code.MatchLdfld<CatControls>("currentState"));
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ceq);
        cursor.Emit<CatControlsJumpWhenLiquidPatch>(OpCodes.Ldsfld, "settingEnabled");
        cursor.Emit(OpCodes.Or);
        cursor.Next.OpCode = OpCodes.Brfalse;
    };

    // ReSharper disable once UnusedMember.Local
    /*private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        int index = -1;
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];

            if(code.opcode == OpCodes.Ldfld &&
               code.OperandIs(AccessTools.Field(typeof(CatControls), "currentState"))) {
                index = i + 1;
            }
        }

        if(index == -1) return codes.AsEnumerable();
        
        codes.InsertRange(index, new[] {
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ceq),
            new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(CatControlsJumpWhenLiquidPatch), "settingEnabled")),
            new CodeInstruction(OpCodes.Or)
        });
        codes[index + 4].opcode = OpCodes.Brfalse;

        return codes.AsEnumerable();
    }*/
}