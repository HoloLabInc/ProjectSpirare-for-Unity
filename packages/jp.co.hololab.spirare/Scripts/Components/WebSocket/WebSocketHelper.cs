using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    public sealed class WebSocketHelper : IDisposable
    {
        private readonly PomlPatchApplier patchApplier;

        private NetWebSocketClient wsClient;

        public WebSocketHelper(PomlPatchApplier patchApplier)
        {
            this.patchApplier = patchApplier;
        }

        public void Dispose()
        {
            wsClient?.Dispose();
        }

        /// <summary>Connects to the specified URL using WebSocket</summary>
        /// <param name="url">The URL of the target host (e.g. "ws://localhost:8000")</param>
        /// <returns></returns>
        public async Task<bool> Connect(string url, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return false;
            }

            Dispose();

            wsClient = new NetWebSocketClient(url);
            wsClient.OnMessageReceived += MessageReceived;
            var result = await wsClient.OpenAsync(ct);

            return result;
        }

        private void MessageReceived(string message)
        {
            patchApplier.ApplyPomlPatchAsync(message).Forget();
        }
    }
}
