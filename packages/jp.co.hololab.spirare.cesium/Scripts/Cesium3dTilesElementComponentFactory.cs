using CesiumForUnity;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class Cesium3dTilesElementComponentFactory : Cesium3dTilesElementFactory
    {
        [SerializeField]
        private Cesium3DTileset cesium3dTilesetPrefab;

        public override GameObject Create(PomlCesium3dTilesElement cesium3dTilesElement, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            if (cesium3dTilesetPrefab == null)
            {
                Debug.LogError("cesium3dTilesetPrefab is null");
                return null;
            }

            var cesium3dTileset = Instantiate(cesium3dTilesetPrefab, parentTransform);
            cesium3dTileset.url = GetTilesetUrl(cesium3dTilesElement);
            return cesium3dTileset.gameObject;
        }

        private static string GetTilesetUrl(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            var url = cesium3dTilesElement.Src;

            // Remove file:// from url
            url = Regex.Replace(url, @"^file://", "");

            url = url.Replace("\\", "/");
            url = url.Replace(" ", "%20");
            return url;
        }
    }
}
