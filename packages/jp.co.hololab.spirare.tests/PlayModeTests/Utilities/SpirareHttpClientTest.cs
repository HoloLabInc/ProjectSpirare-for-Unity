using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using HoloLab.UniWebServer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpirareHttpClientTest
{
    private HttpServerForTest httpServerForTest;
    private ResourceControllerForTest resourceControllerForTest;

    [SetUp]
    public void SetUp()
    {
        resourceControllerForTest = new ResourceControllerForTest()
        {
            DelayMilliseconds = 1000,
        };

        httpServerForTest = new HttpServerForTest();
        httpServerForTest.AddController(resourceControllerForTest);
        httpServerForTest.StartServer();
    }

    [TearDown]
    public void TearDown()
    {
        httpServerForTest.Dispose();
        httpServerForTest = null;
    }

    [Test]
    public async Task GetByteArrayAsync_RequestIsSentOnceWhenCacheEnabled()
    {
        var spirareHttpClient = SpirareHttpClient.Instance;

        var url = $"http://localhost:{httpServerForTest.Port}/resources/test.txt";
        var request1 = spirareHttpClient.GetByteArrayAsync(url, enableCache: true);
        var request2 = spirareHttpClient.GetByteArrayAsync(url, enableCache: true);
        var request3 = spirareHttpClient.GetByteArrayAsync(url, enableCache: true);
        await UniTask.WhenAll(request1, request2, request3);

        Assert.That(resourceControllerForTest.RequestCountDictionary["/resources/test.txt"], Is.EqualTo(1));
    }

    [Test]
    public async Task GetByteArrayAsync_RequestIsSentMultipleTimesWhenCacheDisabled()
    {
        var spirareHttpClient = SpirareHttpClient.Instance;

        var url = $"http://localhost:{httpServerForTest.Port}/resources/test.txt";
        var request1 = spirareHttpClient.GetByteArrayAsync(url, enableCache: false);
        var request2 = spirareHttpClient.GetByteArrayAsync(url, enableCache: false);
        var request3 = spirareHttpClient.GetByteArrayAsync(url, enableCache: false);
        await UniTask.WhenAll(request1, request2, request3);

        Assert.That(resourceControllerForTest.RequestCountDictionary["/resources/test.txt"], Is.EqualTo(3));
    }
}

internal class ResourceControllerForTest : IHttpController
{
    public int DelayMilliseconds;

    public Dictionary<string, int> RequestCountDictionary = new Dictionary<string, int>();

    [Route("/resources/:filename")]
    public async Task<byte[]> GetResources(HttpListenerRequest request, string filename)
    {
        UpdateRequestCount(request);

        await Task.Delay(DelayMilliseconds);
        return Encoding.UTF8.GetBytes(filename);
    }

    private void UpdateRequestCount(HttpListenerRequest request)
    {
        var localPath = request.Url.LocalPath;

        if (RequestCountDictionary.TryGetValue(localPath, out var count))
        {
            RequestCountDictionary[localPath] = count + 1;
        }
        else
        {
            RequestCountDictionary[localPath] = 1;
        }
    }
}

internal class HttpServerForTest : IDisposable
{
    private HttpServer httpServer;
    private Router router;

    public int Port;

    public HttpServerForTest()
    {
        httpServer = new HttpServer();

        router = new Router()
        {
            Context = SynchronizationContext.Current
        };

        // router.AddController(this);

        httpServer.OnRequest += HttpServer_OnRequest;
    }

    public void AddController(IHttpController controller)
    {
        router.AddController(controller);
    }

    public void StartServer()
    {
        Port = GetOpenPort();
        httpServer.Start(Port);
    }

    public void Dispose()
    {
        httpServer.Stop();
    }

    private async void HttpServer_OnRequest(HttpListenerContext context)
    {
        await router.Route(context.Request, context.Response);
    }

    private static int GetOpenPort()
    {
        while (true)
        {
            var port = new System.Random().Next(49152, 65535);
            if (IsFree(port))
            {
                return port;
            }
        }
    }

    private static bool IsFree(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var listeners = properties.GetActiveTcpListeners();
        return listeners.All(listener => listener.Port != port);
    }
}