#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace HoloLab.Spirare.Browser.VisionOS3DMaps.Editor
{
    [InitializeOnLoad]
    internal class StartupAssetCopier
    {
        private class CopyAssetPath
        {
            public string SourcePath;
            public string SourceGuid;
            public string DestinationPath;
        }

        private static readonly string sessionKey = "_SpirareBrowserVisionOS3DMaps_StartupAssetCopier";

        private static readonly CopyAssetPath[] copyAssetPaths = new[]
        {
            new CopyAssetPath
            {
                SourcePath = "Assets/App/Resources~/GooglePhotorealistic3DTilesSourceSettings.asset",
                SourceGuid = "7f948eef0a404a64eb497bdaced032b5",
                DestinationPath = "Assets/Resources/Spirare/GooglePhotorealistic3DTilesSourceSettings.asset",
            },
            new CopyAssetPath
            {
                SourcePath = "Assets/App/Resources~/BaseMapSettings.asset",
                SourceGuid = "7e60df89d8dadeb45a48f2e12f28b446",
                DestinationPath = "Assets/Resources/Spirare/BaseMapSettings.asset",
            }
        };

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

            foreach (var copyAssetPath in copyAssetPaths)
            {
                CopyAssetUtility.TryCopyAsset(copyAssetPath.SourcePath, copyAssetPath.DestinationPath, copyAssetPath.SourceGuid, copyMetaFile: true);
            }
        }
    }
}
#endif
