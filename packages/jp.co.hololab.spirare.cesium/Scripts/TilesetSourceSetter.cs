using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace HoloLab.Spirare.Cesium
{
    [ExecuteAlways]
    [RequireComponent(typeof(Cesium3DTileset))]
    public class TilesetSourceSetter : MonoBehaviour
    {
        [SerializeField]
        private TilesetSourceSettings settings;

        private const string assetLoadPath = "Spirare/TilesetSourceSettings";
        private static string AssetFilePathFromProjectRoot => $"Assets/Resources/{assetLoadPath}.asset";

        private void Awake()
        {
            if (Application.isPlaying)
            {
                SetupTileset();
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                if (settings == null)
                {
                    SetDefaultSettingsAsset();
                }
            }
#endif
        }

        private void SetupTileset()
        {
            if (settings == null)
            {
                Debug.LogError($"TilesetSourceSettings not specified");
                return;
            }

            var tileset = GetComponent<Cesium3DTileset>();
            tileset.url = settings.URL;
        }

#if UNITY_EDITOR
        private void SetDefaultSettingsAsset()
        {
            if (File.Exists(AssetFilePathFromProjectRoot))
            {
                var asset = Resources.Load<TilesetSourceSettings>(assetLoadPath);
                settings = asset;
            }
            else
            {
                CreateParentDirectory(AssetFilePathFromProjectRoot);

                var asset = ScriptableObject.CreateInstance<TilesetSourceSettings>();
                AssetDatabase.CreateAsset(asset, AssetFilePathFromProjectRoot);
                AssetDatabase.SaveAssets();

                settings = asset;
            }
        }

        private static void CreateParentDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }
#endif
    }
}
