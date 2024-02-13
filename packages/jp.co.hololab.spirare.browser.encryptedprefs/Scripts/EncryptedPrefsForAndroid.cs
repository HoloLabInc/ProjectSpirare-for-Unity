#if !UNITY_EDITOR && UNIY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.EncryptedPrefs
{
    public class EncryptedPrefsForAndroid : IEncryptedPrefs
    {
        private readonly AndroidJavaObject encryptedPreferencesManager;

        private static readonly string javaClassName = "jp.co.hololab.spirare.browser.encryptedprefs.EncryptedPreferencesManager";

        public bool IsAvailable
        {
            get
            {
                if (encryptedPreferencesManager == null)
                {
                    return false;
                }


                return encryptedPreferencesManager.Call<bool>("isAvailable");
            }
        }

        public EncryptedPrefsForAndroid(string preferenceName)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaObject("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                        {
                            encryptedPreferencesManager = new AndroidJavaObject(javaClassName, context, preferenceName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public bool SaveString(string key, string value)
        {
            if (encryptedPreferencesManager == null)
            {
                return false;
            }

            try
            {
                return encryptedPreferencesManager.Call<bool>("saveString", key, value);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public bool LoadString(string key, out string value)
        {
            try
            {
                value = encryptedPreferencesManager.Call<string>("loadString", key);
                return value != null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                value = null;
                return false;
            }
        }
    }
}

#endif
