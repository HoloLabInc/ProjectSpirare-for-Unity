using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
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

            if (IsDescendantOfCesiumGeoreference(parentTransform) == false)
            {
                parentTransform = CreateCesiumGeoreference(parentTransform).transform;
            }

            var cesium3dTileset = Instantiate(cesium3dTilesetPrefab, parentTransform);
            cesium3dTileset.url = GetTilesetUrl(cesium3dTilesElement);
            return cesium3dTileset.gameObject;
        }

        private static bool IsDescendantOfCesiumGeoreference(Transform transform)
        {
            if (transform == null)
            {
                return false;
            }

            var cesiumGeoreference = transform.GetComponentInParent<CesiumGeoreference>();
            return cesiumGeoreference != null;
        }

        private static GameObject CreateCesiumGeoreference(Transform transform)
        {
            var georeferenceObject = new GameObject("cesium3dtiles georeference");
            if (transform != null)
            {
                georeferenceObject.transform.SetParent(transform, false);
            }

            georeferenceObject.AddComponent<CesiumGeoreference>();
            georeferenceObject.AddComponent<WorldCoordinateOrigin>();
            georeferenceObject.AddComponent<WorldCoordinateOriginForCesiumGeoreference>();

            return georeferenceObject;
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
