using System;
using System.Reflection;

using Cat;

using HarmonyLib;

using UnityEngine;

namespace CalApi.API.Cat;

public static class CatControlsExtensions {
    public static CatPartManager GetPartManager(this CatControls controls) =>
        (CatPartManager)AccessTools.Field(typeof(CatControls), "catPartManager").GetValue(controls);

    private static readonly FieldInfo currentConfiguration =
        AccessTools.Field(typeof(CatControls), "currentConfiguration");
    private static readonly FieldInfo
        stateConfigurationColor = AccessTools.Field(currentConfiguration.FieldType, "color");
    public static Color GetCurrentConfigurationColor(this CatControls controls) {
        object state = currentConfiguration.GetValue(controls);
        return (Color)stateConfigurationColor.GetValue(state);
    }

    private static readonly Action<CatControls, Color, Color> applyColor = (Action<CatControls, Color, Color>)
        Delegate.CreateDelegate(typeof(Action<CatControls, Color, Color>),
            AccessTools.Method(typeof(CatControls), "ApplyColor"));
    public static void ApplyColor(this CatControls controls, Color color, Color featureColor = default) =>
        applyColor(controls, color, featureColor);

    private static readonly FieldInfo normalStateConfiguration =
        AccessTools.Field(typeof(CatControls), "normalStateConfiguration");
    public static void SetCatNormalColor(this CatControls controls, Color color) {
        object state = normalStateConfiguration.GetValue(controls);
        stateConfigurationColor.SetValue(state, color);
        normalStateConfiguration.SetValue(controls, state);
    }

    private static readonly FieldInfo liquidStateConfiguration =
        AccessTools.Field(typeof(CatControls), "liquidStateConfiguration");
    public static void SetCatLiquidColor(this CatControls controls, Color color) {
        object state = liquidStateConfiguration.GetValue(controls);
        stateConfigurationColor.SetValue(state, color);
        liquidStateConfiguration.SetValue(controls, state);
    }

    private static readonly FieldInfo floatStateConfiguration =
        AccessTools.Field(typeof(CatControls), "floatStateConfiguration");
    public static void SetCatFloatColor(this CatControls controls, Color color) {
        object state = floatStateConfiguration.GetValue(controls);
        stateConfigurationColor.SetValue(state, color);
        floatStateConfiguration.SetValue(controls, state);
    }
}