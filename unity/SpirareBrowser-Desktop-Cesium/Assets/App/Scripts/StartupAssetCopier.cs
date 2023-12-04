#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Desktop.Cesium
{
    [InitializeOnLoad]
    internal class StartupAssetCopier
    {
        private static readonly string sessionKey = "_SpirareBrowserDesktopCesium_StartupAssetCopier";

        private static readonly string sourcePath = "Assets/App/Resources~/GooglePhotorealistic3DTilesSourceSettings.asset";
        private static readonly string sourceGuid = "7f948eef0a404a64eb497bdaced032b5";
        private static readonly string destinationPath = "Assets/Resources/Spirare/GooglePhotorealistic3DTilesSourceSettings.asset";

        private static bool IsNewSession
        {
            get
            {
                if (SessionState.GetBool(sessionKey, false))
                {
                    return false;
                }

                SessionState.SetBool(sessionKey, true);
                return true;
            }
        }

        static StartupAssetCopier()
        {
            if (!IsNewSession || Application.isPlaying)
            {
                return;
            }

            CopyAssetUtility.TryCopyAsset(sourcePath, destinationPath, sourceGuid, copyMetaFile: true);
        }
    }

    public static class CopyAssetUtility
    {
        public static void TryCopyAsset(string sourcePath, string destinationPath, string sourceGuid, bool copyMetaFile)
        {
            if (IsImported(sourceGuid))
            {
                return;
            }

            try
            {
                var destinationFolder = Path.GetDirectoryName(destinationPath);
                Directory.CreateDirectory(destinationFolder);

                File.Copy(sourcePath, destinationPath, false);
                if (copyMetaFile)
                {
                    File.Copy(sourcePath + ".meta", destinationPath + ".meta", false);
                }

                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static bool IsImported(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            return !string.IsNullOrEmpty(path);
        }
    }
}
#endif
