using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CacheManagerTests
{
    [Test]
    public async Task GetValueAsync_FailIfTaskIsNotGenerated()
    {
        var manager = new CacheManager<string>();

        var (Success, _) = await manager.GetValueAsync("key1");
        Assert.That(Success, Is.False);
    }

    [Test]
    public async Task GetValueAsync_SuccessAfterTaskCompleted()
    {
        var manager = new CacheManager<string>();
        manager.GenerateCreationTask("key1");
        manager.CompleteCreationTask("key1", "value1");

        var (Success, Value) = await manager.GetValueAsync("key1");
        Assert.That(Success, Is.True);
        Assert.That(Value, Is.EqualTo("value1"));
    }

    [Test]
    public async Task GetValueAsync_SuccessAfterTaskGenerated()
    {
        var manager = new CacheManager<string>();
        manager.GenerateCreationTask("key1");

        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            manager.CompleteCreationTask("key1", "value1");
        });

        var (Success, Value) = await manager.GetValueAsync("key1");
        Assert.That(Success, Is.True);
        Assert.That(Value, Is.EqualTo("value1"));
    }

    [Test]
    public async Task GetValueAsync_FailIfTaskCanceled()
    {
        var manager = new CacheManager<string>();
        manager.GenerateCreationTask("key1");

        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            manager.CancelCreationTask("key1");
        });

        var (Success, Value) = await manager.GetValueAsync("key1");
        Assert.That(Success, Is.False);
    }

    [Test]
    public async Task GetValueAsync_FailIfCacheCleared()
    {
        var manager = new CacheManager<string>();
        manager.GenerateCreationTask("key1");

        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            manager.ClearCache();
        });

        var (Success, Value) = await manager.GetValueAsync("key1");
        Assert.That(Success, Is.False);
    }
}
