namespace CalApi.DebugMode;

internal interface IDebug {
    public void Load();
    public void SettingsChanged();
    public void CatControlsAwake(Cat.CatControls controls);
    public void CatControlsInputCheck(Cat.CatControls controls);
    public void CatControlsMove(Cat.CatControls controls);
    public void Update();
}
