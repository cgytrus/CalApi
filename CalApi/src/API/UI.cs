using System;

using Language;

using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;
using CalDataEditor = DataEditor;

namespace CalApi.API;

public static class UI {
    public static event EventHandler? initialized;

    private static GameObject? _inputFieldPrefab;

    private static bool _initialized;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public static Font? font { get; private set; }

    public static void Initialize() {
        if(_initialized) return;
        _initialized = true;

        _inputFieldPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor InputField")?.gameObject;
        font = Util.FindResourceOfTypeWithName<Font>("FiraSans-Light");

        initialized?.Invoke(null, EventArgs.Empty);
    }

    // ReSharper disable once UnusedMember.Global
    public static InputField? CreateInputField(RectTransform parent, string nameTranslationString) {
        if(_inputFieldPrefab is null) return null;
        InputField? inputField = Object.Instantiate(_inputFieldPrefab, parent)?.GetComponent<InputField>();
        if(inputField is null) return null;

        inputField.GetComponentInChildren<UILanguageSetter>().key = nameTranslationString;
        inputField.GetComponentInChildren<UILanguageSetter>().UpdateText();
        return inputField;
    }
}
