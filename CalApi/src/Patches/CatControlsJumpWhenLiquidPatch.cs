using Cat;

using Mono.Cecil.Cil;

using MonoMod.Cil;

namespace CalApi.Patches;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CatControlsJumpWhenLiquidPatch : IPatch {
    public static bool enabled;

    public void Apply() => IL.Cat.CatControls.Jump += il => {
        ILCursor cursor = new(il);
        cursor.GotoNext(MoveType.After, code => code.MatchLdfld<CatControls>("currentState"));
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ceq);
        cursor.Emit<CatControlsJumpWhenLiquidPatch>(OpCodes.Ldsfld, nameof(enabled));
        cursor.Emit(OpCodes.Or);
        cursor.Next.OpCode = OpCodes.Brfalse;
    };
}
