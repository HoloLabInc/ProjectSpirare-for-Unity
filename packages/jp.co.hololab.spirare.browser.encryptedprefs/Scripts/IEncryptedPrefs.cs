namespace HoloLab.Spirare.Browser.EncryptedPrefs
{
    internal interface IEncryptedPrefs
    {
        bool IsAvailable { get; }
        bool SaveString(string key, string value);
        bool LoadString(string key, out string value);
    }
}
