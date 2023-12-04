#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
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
