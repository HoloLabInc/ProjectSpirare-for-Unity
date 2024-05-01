using System;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public static class PlayerPrefsUtility
    {
        public static void SetBoolean(string key, bool value)
        {
            var intValue = value ? 1 : 0;
            PlayerPrefs.SetInt(key, intValue);
        }

        public static bool TryGetBoolean(string key, out bool value)
        {
            var intValue = PlayerPrefs.GetInt(key, -1);

            switch (intValue)
            {
                case 0:
                    value = false;
                    return true;
                case 1:
                    value = true;
                    return true;
                default:
                    value = false;
                    return false;
            }
        }
    }
}

