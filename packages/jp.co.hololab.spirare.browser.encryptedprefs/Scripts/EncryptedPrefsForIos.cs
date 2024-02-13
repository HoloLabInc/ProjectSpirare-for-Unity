#if !UNITY_EDITOR && UNITY_IOS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HoloLab.Spirare.Browser.EncryptedPrefs
{
    public class EncryptedPrefsForIos : IEncryptedPrefs
    {
        [DllImport("__Internal", EntryPoint = "SpirareBrowserEncryptedPrefs_KeychainHelper_Save", CallingConvention = CallingConvention.Cdecl)]
        static extern bool KeychainHelper_Save([MarshalAs(UnmanagedType.LPStr), In] string key, [MarshalAs(UnmanagedType.LPStr), In] string value);

        [DllImport("__Internal", EntryPoint = "SpirareBrowserEncryptedPrefs_KeychainHelper_Load", CallingConvention = CallingConvention.Cdecl)]
        static extern void KeychainHelper_Load([MarshalAs(UnmanagedType.LPStr), In] string key, out IntPtr ptr, out int length);

        [DllImport("__Internal", EntryPoint = "SpirareBrowserEncryptedPrefs_DeallocatePointer", CallingConvention = CallingConvention.Cdecl)]
        static extern void DeallocatePointer(IntPtr ptr);

        public bool IsAvailable => true;

        public bool SaveString(string key, string value)
        {
            try
            {
                var result = KeychainHelper_Save(key, value);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public bool LoadString(string key, out string value)
        {
            try
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
            catch (Exception ex)
            {
                Debug.LogException(ex);
                value = null;
                return false;
            }
        }
    }
}

#endif
