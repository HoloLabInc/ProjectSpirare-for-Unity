#if !UNITY_EDITOR && UNITY_IOS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HoloLab.Spirare.Browser.Utilities
{
    public class EncryptedPrefsForIos : IEncryptedPrefs
    {
        [DllImport("__Internal", EntryPoint = "SpirareBrowserUtilities_KeychainHelper_Save", CallingConvention = CallingConvention.Cdecl)]
        static extern bool KeychainHelper_Save([MarshalAs(UnmanagedType.LPStr), In] string key, [MarshalAs(UnmanagedType.LPStr), In] string value);

        [DllImport("__Internal", EntryPoint = "SpirareBrowserUtilities_KeychainHelper_Load", CallingConvention = CallingConvention.Cdecl)]
        static extern void KeychainHelper_Load([MarshalAs(UnmanagedType.LPStr), In] string key, out IntPtr ptr, out int length);

        [DllImport("__Internal", EntryPoint = "SpirareBrowserUtilities_DeallocatePointer", CallingConvention = CallingConvention.Cdecl)]
        static extern void DeallocatePointer(IntPtr ptr);

        public bool IsAvailable => true;

        public bool SetString(string key, string value)
        {
            var result = KeychainHelper_Save(key, value);
            return result;
        }

        public bool GetString(string key, out string value)
        {
            KeychainHelper_Load(key, out IntPtr ptr, out int length);

            if (ptr == IntPtr.Zero)
            {
                value = null;
                return false;
            }

            if (length == 0)
            {
                value = string.Empty;
            }
            else
            {
                value = Marshal.PtrToStringUTF8(ptr, length);
            }

            DeallocatePointer(ptr);
            return true;
        }
    }
}

#endif
