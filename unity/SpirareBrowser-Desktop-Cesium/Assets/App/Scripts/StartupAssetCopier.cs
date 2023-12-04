using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal class StartupAssetCopier
{
    private static readonly string sessionKey = "_SpirareBrowserDesktopCesium_StartupAssetCopier";

    private static readonly string sourcePath = "Assets/App/Resources~/GooglePhotorealistic3DTilesSourceSettings.asset";
    private static readonly string sourceGuid = "";
    private static readonly string destinationPath = "Assets/Resources/test/a/b/GooglePhotorealistic3DTilesSourceSettings.asset";

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
            // return;
        }

        TryCopyAsset();
    }

    private static void TryCopyAsset()
    {
        Debug.Log("Try copy asset");
        if (IsImported(sourceGuid))
        {
            Debug.Log("Is imported");
            return;
        }

        // copy source file to destination file

        Debug.Log("Is not imported");

        try
        {

            // Create dir with recursively
            var destinationFolder = Path.GetDirectoryName(destinationPath);
            Directory.CreateDirectory(destinationFolder);


            File.Copy(sourcePath, destinationPath, false);

            var sourceMetaPath = sourcePath + ".meta";
            var destinationMetaPath = destinationPath + ".meta";
            File.Copy(sourceMetaPath, destinationMetaPath, false);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static bool IsImported(string guid)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        return !string.IsNullOrEmpty(path);
    }
}
