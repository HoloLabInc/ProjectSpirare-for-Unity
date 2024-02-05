using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    internal class LocalFileServer : IDisposable
    {
        private readonly HttpServer httpServer = new HttpServer();

        private int port;

        public int Port => port;

        public void StartOnRandomPort()
        {
            var random = new System.Random();

            while (true)
            {
                port = random.Next(49152, 65535);
                var success = httpServer.Start(port);
                if (success)
                {
                    break;
                }
            }

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

            var basePathKey = "basepath";
            if (request.QueryString.AllKeys.Contains(basePathKey) == false)
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            var basePath = request.QueryString[basePathKey];

            // Remove slash from url path
            var relativePath = request.Url.AbsolutePath.Substring(1);
            var filePath = Path.Combine(basePath, relativePath);

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        await fileStream.CopyToAsync(response.OutputStream);
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
}
