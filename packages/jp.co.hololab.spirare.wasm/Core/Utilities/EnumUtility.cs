using System;

namespace HoloLab.Spirare.Wasm.Core
{
    internal static class EnumUtility
    {
        public static bool TryToEnum<T>(int value, out T enumValue) where T : struct, Enum
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                enumValue = (T)Enum.ToObject(typeof(T), value);
                return true;
            }

            enumValue = default;
            return false;
        }
    }
}
