#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Quest3DMaps
{
    [InitializeOnLoad]
    internal class StartupAssetCopier
    {
        private static readonly string sessionKey = "_SpirareBrowserQuest3DMaps_StartupAssetCopier";

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
}
#endif
