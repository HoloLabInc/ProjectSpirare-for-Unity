using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class Cesium3dTilesElementComponent : SpecificObjectElementComponentBase<PomlCesium3dTilesElement>
    {
        private Cesium3DTileset tileset;

        private static LocalFileServer localFileServer;

        private void Awake()
        {
            tileset = GetComponent<Cesium3DTileset>();
        }

        protected override Task UpdateGameObjectCore()
        {
            StartLoacalFileServerIfNeeded(element);

            var tilesetUrl = GetTilesetUrl(element);
            if (tileset.url != tilesetUrl)
            {
                tileset.url = tilesetUrl;
            }

            // TODO create mask

            return Task.CompletedTask;
        }

        private void StartLoacalFileServerIfNeeded(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            if (localFileServer != null)
            {
                return;
            }

            var url = cesium3dTilesElement.Src;
            if (url.StartsWith("file://"))
            {
                localFileServer = new LocalFileServer();
                localFileServer.StartOnRandomPort();
            }
        }

        private static string GetTilesetUrl(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            var url = cesium3dTilesElement.Src;

            if (url.StartsWith("file://"))
            {
                // Remove file:// from url
                var path = url.Substring(7);

                var directoryPath = Path.GetDirectoryName(path);
                var fileName = Path.GetFileName(path);

                var localServerUrl = $"http://localhost:{localFileServer.Port}/{fileName}?basepath={directoryPath}";
                return localServerUrl;
            }
            else
            {
                return url;
            }
        }
    }
}

