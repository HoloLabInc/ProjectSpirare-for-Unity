using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class Cesium3dTilesElementComponentFactory : Cesium3dTilesElementFactory
    {
        [SerializeField]
        private Cesium3DTileset cesium3dTilesetPrefab;

        private LocalFileServer localFileServer;

        public void OnEnable()
        {
            if (localFileServer == null)
            {
                localFileServer = new LocalFileServer();
                localFileServer.StartOnRandomPort();
            }
        }

        public void OnDisable()
        {
            if (localFileServer != null)
            {
                localFileServer.Dispose();
                localFileServer = null;
            }
        }

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
            var cesium3dtilesElementComponent = cesium3dTileset.gameObject.AddComponent<Cesium3dTilesElementComponent>();
            cesium3dtilesElementComponent.Initialize(cesium3dTilesElement, loadOptions);
            _ = cesium3dtilesElementComponent.UpdateGameObject();
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
    }
}
