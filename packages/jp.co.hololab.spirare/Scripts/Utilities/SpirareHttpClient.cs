using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloLab.Spirare
{
    public struct SpirareHttpClientResult<T>
    {
        public readonly bool Success;
        public readonly T Data;
        public readonly Exception Error;

        public SpirareHttpClientResult(bool success, T data, Exception error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public static SpirareHttpClientResult<T> CreateSuccessResult(T data)
        {
            return new SpirareHttpClientResult<T>(true, data, null);
        }
        public static SpirareHttpClientResult<T> CreateErrorResult(Exception error)
        {
            return new SpirareHttpClientResult<T>(false, default, error);
        }
    }


    public class SpirareHttpClient
    {
        private static readonly string singletonCacheFolderPath = Path.Combine(Application.temporaryCachePath, "SpirareHttpClientCache");

        private static readonly SpirareHttpClient instance = new SpirareHttpClient(singletonCacheFolderPath);

        public static SpirareHttpClient Instance => instance;

        private readonly string cacheFolderPath;
        private readonly ConcurrentDictionary<string, string> cacheFileDictionary = new ConcurrentDictionary<string, string>();


        private SpirareHttpClient(string cacheFolderPath)
        {
            this.cacheFolderPath = cacheFolderPath;
            ClearFolder(cacheFolderPath);
        }

        public async UniTask<SpirareHttpClientResult<byte[]>> GetByteArrayAsync(string url, bool enableCache = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                var ex = new ArgumentException();
                return CreateErrroResult<byte[]>(ex);
            }

            if (IsFileUrl(url))
            {
                return await LoadLocalFile(url);
            }

            // Local cache is only enabled for HTTP requests.
            if (IsHttpUrl(url) == false)
            {
                enableCache = false;
            }

            if (enableCache)
            {
                var cacheResult = await GetCacheAsync(url);
                if (cacheResult.Success)
                {
                    return CreateSuccessResult(cacheResult.Data);
                }
            }

            try
            {
                var request = UnityWebRequest.Get(url);
                var webRequest = await request.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var data = webRequest.downloadHandler.data;

                    if (enableCache)
                    {
                        await SaveCacheAsync(url, data);
                    }
                    return CreateSuccessResult(data);
                }
                else
                {
                    var exception = new Exception(webRequest.error);
                    return CreateErrroResult<byte[]>(exception);
                }
            }
            catch (Exception ex)
            {
                return CreateErrroResult<byte[]>(ex);
            }
        }

        private async UniTask<SpirareHttpClientResult<byte[]>> LoadLocalFile(string url)
        {
            try
            {
                if (url.StartsWith("file://"))
                {
                    url = url.Substring("file://".Length);
                }

                using FileStream stream = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, (int)stream.Length);

                return CreateSuccessResult(fileBytes);
            }
            catch (Exception ex)
            {
                return CreateErrroResult<byte[]>(ex);
            }
        }

        private async UniTask SaveCacheAsync(string url, byte[] data)
        {
            try
            {
                Directory.CreateDirectory(cacheFolderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            var randomFileName = Path.GetRandomFileName();
            var cacheFilePath = Path.Combine(cacheFolderPath, randomFileName);

            try
            {
                using (var fs = File.Open(cacheFilePath, FileMode.CreateNew))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                    cacheFileDictionary.TryAdd(url, cacheFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }

        private async UniTask<(bool Success, byte[] Data)> GetCacheAsync(string url)
        {
            if (cacheFileDictionary.TryGetValue(url, out var cachePath) == false)
            {
                return ((false, default));
            }

            try
            {
                using (var fs = File.OpenRead(cachePath))
                {
                    var data = new byte[fs.Length];
                    await fs.ReadAsync(data, 0, data.Length);
                    return ((true, data));
                }
            }
            catch (Exception)
            {
                return ((false, default));
            }
        }

        private static void ClearFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }

        private static bool IsFileUrl(string url)
        {
            if (url.StartsWith("file://"))
            {
                return true;
            }
            return false;
        }

        private static bool IsHttpUrl(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                return true;
            }
            return false;
        }

        private static SpirareHttpClientResult<T> CreateSuccessResult<T>(T data)
        {
            return SpirareHttpClientResult<T>.CreateSuccessResult(data);
        }

        private static SpirareHttpClientResult<T> CreateErrroResult<T>(Exception ex)
        {
            return SpirareHttpClientResult<T>.CreateErrorResult(ex);
        }
    }
}
