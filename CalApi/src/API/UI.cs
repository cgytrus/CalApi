using System;
using System.Collections.Generic;
using System.Text;

using Language;

using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;
using CalDataEditor = DataEditor;

namespace CalApi.API;

public static class UI {
    public static event EventHandler? initialized;
    private static bool _initialized;

    private static List<string> _copyrightTexts = new(4);

    private static GameObject? _inputFieldPrefab;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public static Font? font { get; private set; }

    internal static void Setup() => On.TitleScreen.Awake += (orig, self) => {
        orig(self);
        Initialize();

        Transform titleScreenStuff = self.transform.Find("Title Screen Stuff");
        Text copyright = titleScreenStuff.Find("Copyright").GetComponent<Text>();
        copyright.verticalOverflow = VerticalWrapMode.Overflow;
        StringBuilder textToAppend = new();
        foreach(string text in _copyrightTexts) {
            textToAppend.Append('\n');
            textToAppend.Append(text);
        }
        copyright.text += textToAppend.ToString();
    };

    private static void Initialize() {
        if(_initialized) return;
        _initialized = true;

        _inputFieldPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor InputField")?.gameObject;
        font = Util.FindResourceOfTypeWithName<Font>("FiraSans-Light");

        initialized?.Invoke(null, EventArgs.Empty);
    }

    public static void AddCopyrightText(string text) => _copyrightTexts.Add(text);

    // ReSharper disable once UnusedMember.Global
    public static InputField? CreateInputField(RectTransform parent, string languageKey) {
        if(_inputFieldPrefab is null) return null;
        InputField? inputField = Object.Instantiate(_inputFieldPrefab, parent)?.GetComponent<InputField>();
        if(inputField is null) return null;

        UILanguageSetter languageSetter = inputField.GetComponentInChildren<UILanguageSetter>();
        languageSetter.key = languageKey;
        languageSetter.UpdateText();
        return inputField;
    }
}
