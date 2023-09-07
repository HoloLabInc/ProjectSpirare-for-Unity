using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal class DynamicSemaphore
    {
        private readonly SemaphoreSlim semaphoreSlim;

        private int maxCount;
        private readonly int maxCountLimit;

        private readonly object lockObject = new object();

        public int CurrentCount => semaphoreSlim.CurrentCount;

        public DynamicSemaphore(int maxCount, int maxCountLimit)
        {
            this.maxCountLimit = maxCountLimit;
            this.maxCount = Mathf.Clamp(maxCount, 0, maxCountLimit);

            semaphoreSlim = new SemaphoreSlim(maxCount, maxCountLimit);
        }

        public void SetMaxCount(int maxCount)
        {
            int diffCount;

            lock (lockObject)
            {
                var newMaxCount = Mathf.Clamp(maxCount, 0, maxCountLimit);
                diffCount = newMaxCount - this.maxCount;
                this.maxCount = newMaxCount;
            }

            if (diffCount > 0)
            {
                semaphoreSlim.Release(diffCount);
            }
            else if (diffCount < 0)
            {
                for (var i = 0; i < -diffCount; i++)
                {
                    semaphoreSlim.WaitAsync();
                }
            }
        }

        public async Task WaitAsync()
        {
            await semaphoreSlim.WaitAsync();
        }

        public void Release()
        {
            semaphoreSlim.Release();
        }
    }
}
