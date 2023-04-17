using System;
using UnityEngine;

namespace HoloLab.Spirare.Wasm.Core.Wasi
{
    public sealed class WasiMiscImplementation
    {
        private readonly System.Random random = new System.Random();

#pragma warning disable IDE1006 // naming style
        public int clock_res_get(IntPtr memoryPtr, uint memoryLength, int id, int timestampPtr)
        {
            long timestamp = 10000;
            MemoryHelper.TryWrite(memoryPtr, memoryLength, timestampPtr, timestamp);
            return (int)Errno.Success;
        }

        public int clock_time_get(IntPtr memoryPtr, uint memoryLength, int id, long precision, int timestampPtr)
        {
            long timestamp;

            if (EnumUtility.TryToEnum(id, out ClockId clockId) == false)
            {
                return (int)Errno.Inval;
            }

            switch (clockId)
            {
                case ClockId.Realtime:
                    var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    timestamp = (long)(((ulong)(DateTime.UtcNow - epochStart).Ticks) * 100);
                    break;
                case ClockId.Monotonic:
                    timestamp = (long)((ulong)Environment.TickCount * 1000 * 1000);
                    break;
                case ClockId.ProcessCputimeId:
                    Debug.LogWarning($"clock_time_get with process_cputime_id is not implemented");
                    return (int)Errno.Inval;
                case ClockId.ThreadCputimeId:
                    Debug.LogWarning($"clock_time_get with thread_cputime_id is not implemented");
                    return (int)Errno.Inval;
                default:
                    return (int)Errno.Inval;
            }

            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, timestampPtr, timestamp))
            {
                return (int)Errno.Success;
            }
            return (int)Errno.Inval;
        }

        public int random_get(IntPtr memoryPtr, uint memoryLength, int bufPtr, int bufLen)
        {
            var bytes = new byte[bufLen];
            random.NextBytes(bytes);
            if (MemoryHelper.TryWriteArray(memoryPtr, memoryLength, bufPtr, bytes))
            {
                return (int)Errno.Success;
            }
            return (int)Errno.Inval;
        }
#pragma warning restore IDE1006 // naming style
    }
}
