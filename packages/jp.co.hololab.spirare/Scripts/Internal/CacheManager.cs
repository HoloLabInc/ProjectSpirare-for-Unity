using System;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace HoloLab.Spirare
{
    internal class CacheManager<T>
    {
        private readonly ConcurrentDictionary<string, T> cacheDictionary
            = new ConcurrentDictionary<string, T>();

        private readonly ConcurrentDictionary<string, UniTaskCompletionSource<T>> cacheCompletionSourceDictionary
            = new ConcurrentDictionary<string, UniTaskCompletionSource<T>>();

        public async UniTask<(bool Success, T Value)> GetValueAsync(string key)
        {
            if (cacheDictionary.TryGetValue(key, out var value))
            {
                return (true, value);
            };

            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                try
                {
                    var result = await completionSource.Task;
                    return (true, result);
                }
                catch (Exception) { }
            }

            return (false, default);
        }

        public bool GenerateCreationTask(string key)
        {
            var taskCompletionSource = new UniTaskCompletionSource<T>();

            try
            {
                if (cacheCompletionSourceDictionary.TryAdd(key, taskCompletionSource))
                {
                    return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public void CompleteCreationTask(string key, T value)
        {
            cacheDictionary.TryAdd(key, value);

            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                completionSource.TrySetResult(value);
                cacheCompletionSourceDictionary.TryRemove(key, out _);
            }
        }

        public void CancelCreationTask(string key)
        {
            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                completionSource.TrySetCanceled();
                cacheCompletionSourceDictionary.TryRemove(key, out _);
            }
        }

        public void ClearCache()
        {
            cacheDictionary.Clear();

            foreach (var completionPair in cacheCompletionSourceDictionary)
            {
                completionPair.Value.TrySetCanceled();
            }
            cacheCompletionSourceDictionary.Clear();
        }
    }
}
