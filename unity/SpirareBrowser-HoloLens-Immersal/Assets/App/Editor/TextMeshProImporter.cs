#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HoloLab.Spirare.Browser.HoloLensImmersal
{
    [InitializeOnLoad]
    internal class TextMeshProImporter
    {
        private static readonly string sessionKey = "_SpirareBrowserHoloLensImmersal_TextMeshProImporter";

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

        static TextMeshProImporter()
        {
            if (!IsNewSession || Application.isPlaying)
            {
                return;
            }

            if (Directory.Exists("Assets/TextMesh Pro"))
            {
                return;
            }

            TMP_PackageUtilities.ImportProjectResourcesMenu();
        }
    }
}
#endif
