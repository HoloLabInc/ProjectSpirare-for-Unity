namespace HoloLab.Spirare.Browser.Utilities
{
    internal class EncryptedPrefsNotSupported : IEncryptedPrefs
    {
        public bool IsAvailable => false;

        public bool SaveString(string key, string value)
        {
            return false;
        }

        public bool LoadString(string key, out string value)
        {
            value = null;
            return false;
        }
    }
}
