using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class TilesetSourceSetter : MonoBehaviour
    {
        [SerializeField]
        private TilesetSourceSettings settings;

        private void Awake()
        {
            SetupTileset();
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
    }
}
