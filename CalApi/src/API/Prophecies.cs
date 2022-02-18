using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BepInEx.Logging;

using HarmonyLib;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using PolyMap;

using ProphecySystem;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using MoveType = MonoMod.Cil.MoveType;

namespace CalApi.API;

// ReSharper disable once UnusedType.Global
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Prophecies {
    public struct Icon {
        public Sprite sprite { get; }
        public Vector3 rotation { get; }

        public Icon(Sprite sprite, Vector3 rotation) {
            this.sprite = sprite;
            this.rotation = rotation;
        }
    }

    public struct Prophecy {
        public Type type { get; }
        public string icon { get; }

        public Prophecy(Type type, string icon) {
            this.type = type;
            this.icon = icon;
        }
    }

    private const string ButtonKeyStart = ".ProphecyButton ";

    private static readonly Dictionary<string, Icon> icons = new();
    private static readonly Dictionary<string, Prophecy> prophecies = new();
    private static readonly Dictionary<Type, string> propheciesByType = new();

    private static int _propheciesButtonsRowsCount;

    internal static void Setup(ManualLogSource logger) {
        Initialize(logger);
        RegisterVanillaProphecies();
    }

    private static void RegisterVanillaProphecies() {
        Type startType = typeof(Prophet);
        string? lookForNamespace = startType.Namespace;
        foreach(DataEditorComponentControlButtons attribute in startType
                    .GetCustomAttributes<DataEditorComponentControlButtons>()) {
            for(int i = 0; i < attribute.methodNames.Length; i++) {
                string methodName = attribute.methodNames[i];
                int prophecyNameStart = "Add".Length;
                int prophecyNameLength = methodName.Length - "Action".Length - prophecyNameStart;
                string prophecyName = methodName.Substring(prophecyNameStart, prophecyNameLength);

                foreach(Type type in startType.Assembly.GetTypes().Where(type =>
                            type.Namespace == lookForNamespace && type.Name == $"{prophecyName}Prophecy"))
                    RegisterProphecy(type.ToString(), type, attribute.iconNames[i]);
            }
        }
    }

    private static void Initialize(ManualLogSource logger) {
        On.DataEditorIcons.GetIconSprite +=
            (orig, name) => icons.TryGetValue(name, out Icon icon) ? icon.sprite : orig(name);
        On.DataEditorIcons.GetIconRotation +=
            (orig, name) => icons.TryGetValue(name, out Icon icon) ? icon.rotation : orig(name);

        _propheciesButtonsRowsCount = typeof(Prophet).GetCustomAttributes<DataEditorComponentControlButtons>().Count();

        int currentProphecyButtonsRow = 0;
        int currentPropheciesIndex = 0;
        On.DataEditor.AddComponentControlButtons += (On.DataEditor.orig_AddComponentControlButtons orig,
            DataEditor self, RectTransform content, MonoBehaviour component, string[] methodNames, string[] iconNames,
            ref float yPosition) => {
            if(component is not Prophet) return orig(self, content, component, methodNames, iconNames, ref yPosition);

            int minPropheciesPerRow = prophecies.Count / _propheciesButtonsRowsCount;
            int additionalProphecies = prophecies.Count % _propheciesButtonsRowsCount;

            int targetProphecies = UpdatePropheciesList(minPropheciesPerRow, additionalProphecies,
                currentProphecyButtonsRow, currentPropheciesIndex, ref methodNames, ref iconNames);

            currentProphecyButtonsRow++;
            currentPropheciesIndex += targetProphecies;

            // ReSharper disable once InvertIf
            if(currentProphecyButtonsRow >= _propheciesButtonsRowsCount) {
                currentProphecyButtonsRow = 0;
                currentPropheciesIndex = 0;
            }

            return orig(self, content, component, methodNames, iconNames, ref yPosition);
        };

        IL.DataEditor.AddComponentControlButtons += il => {
            ILCursor cursor = new(il);
            cursor.GotoNext(code => code.MatchCallvirt<Type>(nameof(Type.GetMethod)));
            cursor.RemoveRange(2);
            cursor.Index++;
            cursor.RemoveRange(5);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Prophecies), nameof(SetButtonOnClick)));
            cursor.GotoPrev(code => code.MatchCallvirt<object>(nameof(object.GetType)));
            cursor.Remove();
            cursor.GotoPrev(code => code.MatchLdloc(6));
            cursor.Remove();
        };

        PatchPerformerRunner(logger);

        IL.ProphecySystem.Prophet.ConvertPropheciesToStringList += il => {
            ILCursor cursor = new(il);
            cursor.GotoNext(MoveType.After, code => code.MatchCallvirt<object>(nameof(object.GetType)));
            cursor.Remove();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Prophecies), nameof(GetProphecyId)));
        };

        IL.ProphecySystem.Prophet.ConvertStringListToProphecies += il => {
            int prophecyType = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Module.ImportReference(typeof(Type))));

            ILCursor cursor = new(il);
            const int caseCount = 9;
            cursor.GotoNext(code => code.MatchCall<Type>(nameof(Type.GetType)));
            // GetType + prophecyTypeStrings[index] + some weird string switch hash shit???? + switch cases
            cursor.RemoveRange(2 + 5 + 43 + 5 * caseCount);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Prophecies), nameof(GetProphecyType)));
            cursor.Emit(OpCodes.Stloc, prophecyType);
            cursor.GotoNext(code =>
                code.MatchCall(AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject))));
            cursor.Index++;
            cursor.Remove();
            cursor.Emit(OpCodes.Ldloc, prophecyType);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent),
                new[] { typeof(Type) }));
            cursor.GotoNext(code => code.MatchBr(out _));
            // 1+6+9 (16) instructions per case but we utilized one case so it's 1 less
            cursor.RemoveRange((1 + 6 + 9) * (caseCount - 1));
        };
    }

    private static void PatchPerformerRunner(ManualLogSource logger) {
        void Manipulator(ILContext il) {
            int performerCoroutine = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Module.ImportReference(typeof(Coroutine))));
            int prophecy = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Module.ImportReference(typeof(BaseProphecy))));

            ILCursor cursor = new(il);

            // get the switch labels to add another `yield return ...` later
            ILLabel[]? switchLabels = null;
            cursor.GotoNext(code => code.MatchSwitch(out switchLabels));
            ILCursor switchCursor = new(cursor);
            if(switchLabels is null) {
                logger.LogError($"{nameof(switchLabels)} is null");
                return;
            }

            // find a GetType call, which are only used in the loop and ifs we need and go back a bit to get the code
            // that gets the current prophecy
            cursor.GotoNext(code => code.MatchCallvirt<object>(nameof(object.GetType)));
            cursor.Index -= 5;
            ILCursor ifCursor = new(cursor);

            // find the first return after that which is used in a `yield return ...` and get the whole code for it
            cursor.GotoNext(code => code.MatchRet());
            cursor.Index -= 5;
            ILCursor yieldReturnCursor = new(cursor);

            // go to the end of the loop
            cursor.GotoNext(MoveType.After, code => code.MatchCall<Music>(nameof(Music.QueueTrack)));
            ILCursor loopEndCursor = new(cursor);

            // find and save the label that jumps to the end of the loop
            ILLabel? loopEndLabel = null;
            loopEndCursor.GotoPrev(code => code.MatchBrfalse(out loopEndLabel));
            if(loopEndLabel is null) {
                logger.LogError($"{nameof(loopEndLabel)} is null");
                return;
            }

            // important to not return after this as we're now emitting IL here!!

            // if(prophet.prophecies[i] is BaseProphecy prophecy)
            for(int i = 0; i < 5; i++) {
                Instruction code = ifCursor.Instrs[ifCursor.Index + i];
                cursor.Emit(code.OpCode, code.Operand);
            }
            cursor.Index -= 5; // fuck.
            loopEndCursor.Instrs[loopEndCursor.Index].Operand = cursor.MarkLabel();
            cursor.Index += 5;
            cursor.Emit(OpCodes.Isinst, typeof(BaseProphecy));
            cursor.Emit(OpCodes.Stloc, prophecy);
            cursor.Emit(OpCodes.Ldloc, prophecy);
            cursor.Emit(OpCodes.Brfalse_S, loopEndLabel);

            // ... StartCoroutine(prophecy.Performer());
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc, prophecy);
            cursor.Emit<BaseProphecy>(OpCodes.Callvirt, nameof(BaseProphecy.Performer));
            cursor.Emit(OpCodes.Call,
                AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine),
                    new[] { typeof(IEnumerator) }));
            // yield return ...
            cursor.Emit(OpCodes.Stloc, performerCoroutine);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, performerCoroutine);
            for(int i = 0; i < 9; i++) {
                Instruction code = yieldReturnCursor.Instrs[yieldReturnCursor.Index + i];
                switch(i) {
                    case 2:
                        cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)switchLabels.Length);
                        break;
                    case 6:
                        Array.Resize(ref switchLabels, switchLabels.Length + 1);
                        switchLabels[switchLabels.Length - 1] = cursor.MarkLabel();
                        switchCursor.Instrs[switchCursor.Index].Operand = switchLabels;
                        cursor.Emit(code.OpCode, code.Operand);
                        break;
                    default:
                        cursor.Emit(code.OpCode, code.Operand);
                        break;
                }
            }
        }

        IDetour tellDetour = new ILHook(AccessTools.Method(AccessTools.TypeByName("<Tell>d__42"), "MoveNext"),
            Manipulator);
        tellDetour.Apply();
    }

    private static int UpdatePropheciesList(int minPropheciesPerRow, int additionalProphecies, int row, int startIndex,
        ref string[] methodNames, ref string[] iconNames) {
        int targetProphecies = row < additionalProphecies ? minPropheciesPerRow + 1 :
            minPropheciesPerRow;

        Array.Resize(ref methodNames, targetProphecies);
        Array.Resize(ref iconNames, targetProphecies);

        int currentColumn = 0;
        int index = 0;
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach(KeyValuePair<string, Prophecy> prophecy in prophecies) {
            if(index++ < startIndex) continue;

            methodNames[currentColumn] = $"{ButtonKeyStart}{prophecy.Key}";
            iconNames[currentColumn] = prophecy.Value.icon;

            if(currentColumn >= methodNames.Length - 1) break;
            currentColumn++;
        }

        return targetProphecies;
    }

    private static void SetButtonOnClick(MonoBehaviour component, string methodName, Button button) {
        if(!methodName.StartsWith(ButtonKeyStart, StringComparison.Ordinal)) {
            MethodInfo? method = component.GetType().GetMethod(methodName);
            button.onClick.AddListener((UnityAction)(() => method?.Invoke(component, Array.Empty<object>())));
            return;
        }

        string prophecyId = methodName.Substring(ButtonKeyStart.Length);
        Prophet prophet = (Prophet)component;
        button.onClick.AddListener((UnityAction)(() => {
            if(!prophecies.TryGetValue(prophecyId, out Prophecy prophecy)) return;
            prophet.prophecies.Add((MonoBehaviour)prophet.gameObject.AddComponent(prophecy.type));
            DataEditor.EditItem(prophet.GetComponent<Item>());
        }));
    }

    public static void AddIcon(string name, Icon icon) => icons.Add(name, icon);

    public static void RegisterProphecy<T>(string id, string icon) => RegisterProphecy(id, typeof(T), icon);
    public static void RegisterProphecy(string id, Type type, string icon) =>
        RegisterProphecy(id, new Prophecy(type, icon));
    public static void RegisterProphecy(string id, Prophecy prophecy) {
        prophecies.Add(id, prophecy);
        propheciesByType.Add(prophecy.type, id);
    }

    public static Type GetProphecyType(string id) => prophecies[id].type;
    public static string? GetProphecyId(Type type) => propheciesByType.TryGetValue(type, out string id) ? id : null;
}
