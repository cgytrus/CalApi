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

    private static readonly List<string> copyrightTexts = new(4);

    private static GameObject? _buttonPrefab;
    //private static GameObject? _controlButtonPrefab;
    private static GameObject? _dropdownPrefab;
    private static GameObject? _iconTogglePrefab;
    private static GameObject? _inputFieldPrefab;
    //private static GameObject? _presetButtonPrefab;
    private static GameObject? _sliderPrefab;
    //private static GameObject? _stringDisplayPrefab;
    private static GameObject? _togglePrefab;

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
        foreach(string text in copyrightTexts) {
            textToAppend.Append('\n');
            textToAppend.Append(text);
        }
        copyright.text += textToAppend.ToString();
    };

    private static void Initialize() {
        if(_initialized) return;
        _initialized = true;

        _buttonPrefab = Util.FindResourceOfTypeWithName<Button>("Data Editor Button")?.gameObject;
        //_controlButtonPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor Control Button")?.gameObject;
        _dropdownPrefab = Util.FindResourceOfTypeWithName<Dropdown>("Data Editor Dropdown")?.gameObject;
        _iconTogglePrefab = Util.FindResourceOfTypeWithName<Toggle>("Data Editor Icon Toggle")?.gameObject;
        _inputFieldPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor InputField")?.gameObject;
        //_presetButtonPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor Preset Button")?.gameObject;
        _sliderPrefab = Util.FindResourceOfTypeWithName<Slider>("Data Editor Slider")?.gameObject;
        //_stringDisplayPrefab = Util.FindResourceOfTypeWithName<InputField>("Data Editor String Display")?.gameObject;
        _togglePrefab = Util.FindResourceOfTypeWithName<Toggle>("Data Editor Toggle")?.gameObject;

        Debug.Log(_buttonPrefab);
        Debug.Log(_dropdownPrefab);
        Debug.Log(_iconTogglePrefab);
        Debug.Log(_inputFieldPrefab);
        Debug.Log(_sliderPrefab);
        Debug.Log(_togglePrefab);

        font = Util.FindResourceOfTypeWithName<Font>("FiraSans-Light");

        initialized?.Invoke(null, EventArgs.Empty);
    }

    public static void AddCopyrightText(string text) => copyrightTexts.Add(text);

    // ReSharper disable once UnusedMember.Global
    public static Button? CreateButton(RectTransform parent, string languageKey) {
        if(_buttonPrefab is null) return null;
        Button? element = Object.Instantiate(_buttonPrefab, parent)?.GetComponent<Button>();
        if(element is null) return null;

        UILanguageSetter languageSetter = element.GetComponentInChildren<UILanguageSetter>();
        languageSetter.key = languageKey;
        languageSetter.UpdateText();
        return element;
    }

    // ReSharper disable once UnusedMember.Global
    public static Dropdown? CreateDropdown(RectTransform parent) {
        if(_dropdownPrefab is null) return null;
        Dropdown? element = Object.Instantiate(_dropdownPrefab, parent)?.GetComponent<Dropdown>();
        return element ? element : null;
    }

    // ReSharper disable once UnusedMember.Global
    public static InputField? CreateInputField(RectTransform parent, string languageKey) {
        if(_inputFieldPrefab is null) return null;
        InputField? element = Object.Instantiate(_inputFieldPrefab, parent)?.GetComponent<InputField>();
        if(element is null) return null;

        UILanguageSetter languageSetter = element.GetComponentInChildren<UILanguageSetter>();
        languageSetter.key = languageKey;
        languageSetter.UpdateText();
        return element;
    }

    // ReSharper disable once UnusedMember.Global
    public static Toggle? CreateToggle(RectTransform parent, string languageKey) {
        if(_togglePrefab is null) return null;
        Toggle? element = Object.Instantiate(_togglePrefab, parent)?.GetComponent<Toggle>();
        if(element is null) return null;

        UILanguageSetter languageSetter = element.GetComponentInChildren<UILanguageSetter>();
        languageSetter.key = languageKey;
        languageSetter.UpdateText();
        return element;
    }

    // ReSharper disable once UnusedMember.Global
    public static Toggle? CreateIconToggle(RectTransform parent, Sprite sprite) {
        if(_iconTogglePrefab is null) return null;
        Toggle? element = Object.Instantiate(_iconTogglePrefab, parent)?.GetComponent<Toggle>();
        if(element is null) return null;

        element.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        element.transform.GetChild(1).GetComponent<Image>().sprite = sprite;
        return element;
    }

    // ReSharper disable once UnusedMember.Global
    public static Slider? CreateSlider(RectTransform parent, bool divideByTen, bool showNumbers, bool zeroIsInfinite) {
        if(_sliderPrefab is null) return null;
        Slider? element = Object.Instantiate(_sliderPrefab, parent)?.GetComponent<Slider>();
        if(element is null) return null;

        foreach(SliderDisplay componentsInChild in element.GetComponentsInChildren<SliderDisplay>()) {
            componentsInChild.divideByTen = divideByTen;
            componentsInChild.zeroIsInfinite = zeroIsInfinite;
        }

        if(showNumbers) return element;
        foreach(Text componentsInChild in element.GetComponentsInChildren<Text>())
            if(!componentsInChild.gameObject.name.Contains("Label"))
                componentsInChild.enabled = false;

        return element;
    }
}
