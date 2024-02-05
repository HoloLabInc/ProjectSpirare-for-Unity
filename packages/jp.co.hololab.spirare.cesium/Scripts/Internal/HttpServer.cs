using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HoloLab.Spirare.Cesium
{
    internal class HttpServer
    {
        private HttpListener httpListener;
        private CancellationTokenSource tokenSource;

        public event Action<HttpListenerContext> OnRequest;

        public bool Start(int port)
        {
            httpListener = new HttpListener();
            var uri = $"http://*:{port}/";
            httpListener.Prefixes.Add(uri);

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            try
            {
                httpListener.Start();
            }
            catch (Exception)
            {
                return false;
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    var context = await httpListener.GetContextAsync();
                    OnRequest?.Invoke(context);
                }
            }, token);

            return true;
        }

        public void Stop()
        {
            tokenSource?.Cancel();
            httpListener?.Stop();

            tokenSource = null;
            httpListener = null;
        }
    }
}
