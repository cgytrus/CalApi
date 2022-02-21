using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Cat;

using HarmonyLib;

using UnityEngine;

namespace CalApi.API.Cat;

public static class CatPartManagerExtensions {
    // https://www.youtube.com/watch?v=er9nD-usM1A
    private static readonly Func<CatPartManager, List<GameObject>> getParts =
        (Func<CatPartManager, List<GameObject>>)Delegate.CreateDelegate(
            typeof(Func<CatPartManager, List<GameObject>>),
            AccessTools.PropertyGetter(typeof(CatPartManager), "Parts"));
    public static List<GameObject> GetParts(this CatPartManager self) => getParts(self);

    private static readonly Func<CatPartManager, List<Rigidbody2D>> getPartRigidbodies =
        (Func<CatPartManager, List<Rigidbody2D>>)Delegate.CreateDelegate(
            typeof(Func<CatPartManager, List<Rigidbody2D>>),
            AccessTools.PropertyGetter(typeof(CatPartManager), "PartRigidbodies"));
    public static List<Rigidbody2D> GetPartRigidbodies(this CatPartManager self) => getPartRigidbodies(self);

    private static readonly Action<CatPartManager, CatPartManager.CatFeature, bool> setCatFeatureVisibility =
        (Action<CatPartManager, CatPartManager.CatFeature, bool>)Delegate.CreateDelegate(
            typeof(Action<CatPartManager, CatPartManager.CatFeature, bool>),
            AccessTools.Method(typeof(CatPartManager), "SetCatFeatureVisibility"));
    public static void SetCatFeatureVisibility(this CatPartManager self, CatPartManager.CatFeature feature,
        bool setTo) => setCatFeatureVisibility(self, feature, setTo);

    private static readonly Action<CatPartManager, Vector3> moveCat =
        (Action<CatPartManager, Vector3>)Delegate.CreateDelegate(
            typeof(Action<CatPartManager, Vector3>),
            AccessTools.Method(typeof(CatPartManager), "MoveCat"));
    public static void MoveCat(this CatPartManager self, Vector3 position) => moveCat(self, position);
}
