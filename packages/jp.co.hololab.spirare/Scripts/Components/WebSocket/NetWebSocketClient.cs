using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal class NetWebSocketClient : IDisposable
    {
        private readonly ClientWebSocket ws;
        private static readonly int messageBufferSize = 1024;
        private static readonly AsyncLock asyncLock = new AsyncLock();

        private string url;

        public event Action<string> OnError;
        public event Action OnOpened;
        public event Action<string> OnMessageReceived;

        public class ClosedEventArgs
        {
            public int StatusCode;
            public string ErrorMessage;
        }

        public event Action<ClosedEventArgs> OnClosed;

        public NetWebSocketClient(string url)
        {
            ws = new ClientWebSocket();
            this.url = url;
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await CloseAsync();
                ws.Dispose();
            });
        }

        public async Task<bool> OpenAsync(CancellationToken ct = default)
        {
            await ws.ConnectAsync(new Uri(url), ct);

            if (ws.State == WebSocketState.Open)
            {
                _ = Task.Run(async () =>
                {
                    await ReceiveMessageLoop(ct);
                });
                OnOpened?.Invoke();
                return true;
            }
            else
            {
                OnError?.Invoke("Connection failed");
                return false;
            }
        }

        public async Task CloseAsync()
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }

        public async Task<bool> SendAsync(string message)
        {
            try
            {
                using (await asyncLock.LockAsync())
                {
                    if (ws.State != WebSocketState.Open)
                    {
                        return false;
                    }

                    var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await ws.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        private async Task ReceiveMessageLoop(CancellationToken ct)
        {
            var buff = new ArraySegment<byte>(new byte[messageBufferSize]);
            var ackMessage = Encoding.UTF8.GetBytes("ack");

            while (ws.State == WebSocketState.Open)
            {
                var messageData = new byte[0];
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    // TODO cancel when disconnected
                    var ret = await ws.ReceiveAsync(buff, ct);
                    if (ret.CloseStatus.HasValue)
                    {
                        var statusCode = (int)ret.CloseStatus.Value;
                        InvokeCloseEvent(statusCode, ret.CloseStatusDescription);
                        return;
                    }
                    var length = messageData.Length;
                    Array.Resize(ref messageData, length + ret.Count);
                    Buffer.BlockCopy(buff.Array, 0, messageData, length, ret.Count);

                    if (ret.EndOfMessage)
                    {
                        await ws.SendAsync(ackMessage, WebSocketMessageType.Text, true, ct);
                        break;
                    }
                }
                try
                {
                    var message = Encoding.UTF8.GetString(messageData);
                    OnMessageReceived?.Invoke(message);
                }
                catch (Exception)
                {
                    // Ignore invalid message 
                }
            }
        }

        private void InvokeCloseEvent(int statusCode, string message)
        {
            var eventArgs = new ClosedEventArgs()
            {
                StatusCode = statusCode,
                ErrorMessage = message
            };
            OnClosed?.Invoke(eventArgs);
        }
    }
}
