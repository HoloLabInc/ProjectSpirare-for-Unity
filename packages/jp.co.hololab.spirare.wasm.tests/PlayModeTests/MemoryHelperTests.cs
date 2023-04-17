using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HoloLab.Spirare.Wasm.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class MemoryHelperTests
{
    [Test]
    public void TryRead_Byte()
    {
        var memory = new byte[10];
        memory[1] = 100;
        memory[5] = 31;

        var (memoryPtr, memoryLength) = BytesToPtr(memory);

        {
            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 1, out byte data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(100));
        }
        {
            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 5, out byte data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(31));
        }
    }

    [Test]
    public void TryRead_Int32()
    {
        var memory = new byte[10];
        CopyBytes(memory, 1, BitConverter.GetBytes(100000));
        CopyBytes(memory, 6, BitConverter.GetBytes(-1));

        var (memoryPtr, memoryLength) = BytesToPtr(memory);

        {
            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 1, out int data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(100000));
        }
        {
            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 6, out int data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(-1));
        }
    }

    [Test]
    public void TryRead_Bytes()
    {
        var memory = new byte[]
        {
            0, 1, 2, 3, 4, 5, 6
        };

        var (memoryPtr, memoryLength) = BytesToPtr(memory);

        {
            var result = MemoryHelper.TryReadBytes(memoryPtr, memoryLength, 1, 2, out var data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(new byte[] { 1, 2 }));
        }
        {
            var result = MemoryHelper.TryReadBytes(memoryPtr, memoryLength, 2, 4, out var data);
            Assert.That(result, Is.True);
            Assert.That(data, Is.EqualTo(new byte[] { 2, 3, 4, 5 }));
        }
    }

    private struct TestStruct
    {
        public int IntValue;
        public float FloatValue;
    }

    [Test]
    public void TryRead_Struct()
    {
        var memory = new byte[10];

        var testData1 = new TestStruct
        {
            IntValue = -1,
            FloatValue = 2.3f
        };

        var (memoryPtr, memoryLength) = BytesToPtr(memory);
        Marshal.StructureToPtr(testData1, memoryPtr + 2, true);

        {
            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 2, out TestStruct data);
            Assert.That(result, Is.True);
            Assert.That(data.IntValue, Is.EqualTo(testData1.IntValue));
            Assert.That(data.FloatValue, Is.EqualTo(testData1.FloatValue));
        }
    }

    [Test]
    public void TryReadUtf8()
    {
        var memory = new byte[255];
        var textBytes = Encoding.UTF8.GetBytes("“ú–{ŒêText");
        CopyBytes(memory, 1, textBytes);

        var (memoryPtr, memoryLength) = BytesToPtr(memory);

        {
            var result = MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, 1, textBytes.Length, out var text);
            Assert.That(result, Is.True);
            Assert.That(text, Is.EqualTo("“ú–{ŒêText"));
        }
    }


    private static void CopyBytes(byte[] memory, int offset, byte[] data)
    {
        Array.Copy(data, 0, memory, offset, data.Length);
    }

    private static (IntPtr Pointer, uint Length) BytesToPtr(byte[] data)
    {
        IntPtr memoryPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * data.Length);
        Marshal.Copy(data, 0, memoryPtr, data.Length);

        return (memoryPtr, (uint)data.Length);
    }
}
