namespace HoloLab.Spirare.Browser.Utilities
{
    internal interface IEncryptedPrefs
    {
        bool IsAvailable { get; }
        bool SaveString(string key, string value);
        bool LoadString(string key, out string value);
    }
}
