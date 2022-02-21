using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using BepInEx.Logging;

using CalApi.Patches;

using UnityEngine;

using Object = UnityEngine.Object;

namespace CalApi.API;

public static class Util {
    internal static ManualLogSource? logger { get; set; }

    private static BindingFlags allBindingFlags => BindingFlags.Public
                                                   | BindingFlags.NonPublic
                                                   | BindingFlags.Instance
                                                   | BindingFlags.Static
                                                   | BindingFlags.GetField
                                                   | BindingFlags.SetField
                                                   | BindingFlags.GetProperty
                                                   | BindingFlags.SetProperty;

    public static readonly string moddedStreamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "Modded");

    public static Object? FindResourceOfTypeWithName(Type type, string name) {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach(Object obj in Resources.FindObjectsOfTypeAll(type)) {
            if(obj.name != name) continue;
            return obj;
        }

        return null;
    }

    public static T? FindResourceOfTypeWithName<T>(string name) where T : Object =>
        (T?)FindResourceOfTypeWithName(typeof(T), name);

    public static void ApplyAllPatches() {
        Assembly assembly = Assembly.GetCallingAssembly();
        ForEachImplementation(assembly, typeof(IPatch), Action);
        void Action(Type type) {
            IPatch? patch = null;
            try { patch = Activator.CreateInstance(type) as IPatch; }
            catch(TargetInvocationException ex) { logger?.LogError(ex); } // constructor exception
            catch { /* ignored */ }

            try { patch?.Apply(); }
            catch(Exception ex) { logger?.LogError(ex); }

            ForEachImplementation(assembly, type, Action);
        }
    }

    public static void ForEachImplementation(Type interfaceType, Action<Type> action) {
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            ForEachImplementation(assembly, interfaceType, action);
    }

    public static void ForEachImplementation(Assembly assembly, Type baseType, Action<Type> action) {
        try {
            foreach(Type type in assembly.GetTypes())
                if(new List<Type>(type.GetNestedTypes(allBindingFlags)).Contains(baseType) ||
                   new List<Type>(type.GetInterfaces()).Contains(baseType))
                    action(type);
        }
        catch(ReflectionTypeLoadException ex) {
            LogReflectionTypeLoadException(ex);
        }
    }

    private static void LogReflectionTypeLoadException(ReflectionTypeLoadException ex) {
        if(logger is null) Debug.LogWarning(ex);
        else logger.LogWarning(ex);
        foreach(Exception loaderException in ex.LoaderExceptions) {
            if(logger is null) Debug.LogWarning(loaderException);
            else logger.LogWarning(loaderException);
        }
    }
}
