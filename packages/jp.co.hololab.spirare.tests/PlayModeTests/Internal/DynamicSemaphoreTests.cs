using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DynamicSemaphoreTest
{
    [Test]
    public async Task CurrentCountIsChanged()
    {
        var semaphore = new DynamicSemaphore(5, 20);
        Assert.That(semaphore.CurrentCount, Is.EqualTo(5));

        await semaphore.WaitAsync();
        Assert.That(semaphore.CurrentCount, Is.EqualTo(4));

        semaphore.Release();
        Assert.That(semaphore.CurrentCount, Is.EqualTo(5));
    }

    [Test]
    public async Task SetMaxCount_WaitAsyncCompleted()
    {
        var semaphore = new DynamicSemaphore(0, 20);

        bool isCompleted = false;
        _ = Task.Run(async () =>
        {
            await semaphore.WaitAsync();
            isCompleted = true;
        });

        Assert.IsFalse(isCompleted);

        semaphore.SetMaxCount(1);
        await Task.Delay(1);
        Assert.IsTrue(isCompleted);
    }
}
