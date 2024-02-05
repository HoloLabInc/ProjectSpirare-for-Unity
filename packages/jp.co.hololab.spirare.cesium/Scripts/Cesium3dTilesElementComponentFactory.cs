using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
            cesium3dTileset.url = GetTilesetUrl(cesium3dTilesElement);
            Debug.Log("Load 3d tileset: " + cesium3dTileset.url);
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

        private string GetTilesetUrl(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            var url = cesium3dTilesElement.Src;

            Debug.Log(url);
            // Remove file:// from url
            if (url.StartsWith("file://"))
            {
                var path = url.Substring(7);
                Debug.Log("path: " + path);

                // get directory from path
                var directoryPath = Path.GetDirectoryName(path);
                var fileName = Path.GetFileName(path);

                var localServerUrl = $"http://localhost:{localFileServer.Port}/{fileName}?path={directoryPath}";
                return localServerUrl;
            }

            /*
            url = Regex.Replace(url, @"^file://", "");

            url = url.Replace("\\", "/");
            url = url.Replace(" ", "%20");
            */
            return url;
        }
    }

    internal class LocalFileServer : IDisposable
    {
        private readonly HttpServer httpServer = new HttpServer();

        private int port;

        public int Port => port;

        public void StartOnRandomPort()
        {
            port = 12345;
            httpServer.Start(port);

            httpServer.OnRequest += HttpServer_OnRequest;
        }

        public void Dispose()
        {
            httpServer.Stop();
        }

        private async void HttpServer_OnRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // get path query from request
            var query = request.Url.Query;
            //request.QueryString["aa"]

            var pathKey = "path";
            if (request.QueryString.AllKeys.Contains(pathKey) == false)
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            var basePath = request.QueryString[pathKey];

            Debug.LogWarning(request.RawUrl);
            Debug.LogWarning(request.Url.AbsolutePath);
            Debug.LogWarning(basePath);
            // TODO return file

            // Remove slash from url path
            var relativePath = request.Url.AbsolutePath.Substring(1);
            var filePath = Path.Combine(basePath, relativePath);
            Debug.LogWarning(filePath);

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        await fileStream.CopyToAsync(response.OutputStream);
                        Debug.LogWarning(fileStream.Length);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    response.StatusCode = 404;
                }
            }

            response.Close();
        }
    }

    internal class HttpServer
    {
        private HttpListener httpListener;
        private CancellationTokenSource tokenSource;

        public event Action<HttpListenerContext> OnRequest;

        public void Start(int port)
        {
            httpListener = new HttpListener();
            var uri = $"http://*:{port}/";
            httpListener.Prefixes.Add(uri);

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            Task.Run(async () =>
            {
                httpListener.Start();
                while (true)
                {
                    var context = await httpListener.GetContextAsync();
                    OnRequest?.Invoke(context);
                }
            }, token);
        }

        public void Stop()
        {
            tokenSource?.Cancel();
            httpListener?.Stop();
        }
    }
}
