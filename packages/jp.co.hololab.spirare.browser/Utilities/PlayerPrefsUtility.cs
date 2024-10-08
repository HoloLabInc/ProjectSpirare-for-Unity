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

        public static bool TryGetFloat(string key, out float value)
        {
            value = PlayerPrefs.GetFloat(key, float.NaN);
            if (float.IsNaN(value))
            {
                value = 0;
                return false;
            }

            return true;
        }

        public static void SetEnum<T>(string key, T value) where T : Enum
        {
            PlayerPrefs.SetInt(key, Convert.ToInt32(value));
        }

        public static bool TryGetEnum<T>(string key, out T value) where T : Enum
        {
            var intValue = PlayerPrefs.GetInt(key, int.MinValue);
            if (Enum.IsDefined(typeof(T), intValue))
            {
                value = (T)(object)intValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}

