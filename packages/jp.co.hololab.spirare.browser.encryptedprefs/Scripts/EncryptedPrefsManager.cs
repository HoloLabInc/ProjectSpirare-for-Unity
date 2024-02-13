namespace HoloLab.Spirare.Browser.EncryptedPrefs
{
    public static class EncryptedPrefsManager
    {
        private static readonly IEncryptedPrefs encryptedPrefsImpl =
#if !UNITY_EDITOR && UNITY_IOS
            new EncryptedPrefsForIos();
#elif !UNITY_EDITOR && UNITY_ANDROID
            new EncryptedPrefsForAndroid("spirare_browser_encrypted_prefs");
#else
            new EncryptedPrefsNotSupported();
#endif

        public static bool IsAvailable => encryptedPrefsImpl.IsAvailable;

        public static bool SaveString(string key, string value)
        {
            return encryptedPrefsImpl.SaveString(key, value);
        }

        public static bool LoadString(string key, out string value)
        {
            return encryptedPrefsImpl.LoadString(key, out value);
        }
    }
}
