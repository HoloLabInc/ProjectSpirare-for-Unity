﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly int maxConnectionsLimit = 100;

        private int maxConnections = 6;

        public int MaxConnections
        {
            set
            {
                maxConnections = Mathf.Clamp(value, 0, maxConnectionsLimit);
                semaphore.SetMaxCount(maxConnections);
            }
            get
            {
                return maxConnections;
            }
        }

        private readonly DynamicSemaphore semaphore;

        private static readonly string singletonCacheFolderPath = Path.Combine(Application.temporaryCachePath, "SpirareHttpClientCache");

        private static SpirareHttpClient instance;

        public static SpirareHttpClient Instance
        {
            set
            {
                instance = value;
            }
            get
            {
                if (instance == null)
                {
                    instance = new SpirareHttpClient(singletonCacheFolderPath, true);
                }

                return instance;
            }
        }

        private readonly string cacheFolderPath;

        private readonly ConcurrentDictionary<string, UniTask<string>> cacheDownloadTaskDictionary = new ConcurrentDictionary<string, UniTask<string>>();

        private static readonly HashAlgorithm hasher = MD5.Create();

        public SpirareHttpClient(string cacheFolderPath, bool clearCacheOnInitialize = false)
        {
            semaphore = new DynamicSemaphore(maxConnections, maxConnectionsLimit);

            this.cacheFolderPath = cacheFolderPath;

            if (clearCacheOnInitialize)
            {
                ClearCache();
            }
            else
            {
                CreateDirectory(cacheFolderPath);
            }
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
                var cacheResult = await GetCacheDataAsync(url);
                if (cacheResult.Success)
                {
                    return CreateSuccessResult(cacheResult.Data);
                }
            }

            var downloadTaskSource = enableCache ? SetCacheDownloadTaskSource(url) : null;

            var downloadHandler = new DownloadHandlerBuffer();
            var result = await SendGetRequestAsync(url, downloadHandler);
            if (result.Success)
            {
                var data = downloadHandler.data;
                result.Data.Dispose();

                if (data == null)
                {
                    if (enableCache)
                    {
                        CompleteCacheDownloadTaskSouce(url, downloadTaskSource, result: null);
                    }
                    return CreateErrroResult<byte[]>(new Exception("Downloaded data is empty"));
                }
                else
                {
                    if (enableCache)
                    {
                        var savedCachePath = await SaveCacheAsync(url, data);
                        CompleteCacheDownloadTaskSouce(url, downloadTaskSource, savedCachePath);
                    }
                    return CreateSuccessResult(data);
                }
            }
            else
            {
                if (enableCache)
                {
                    CompleteCacheDownloadTaskSouce(url, downloadTaskSource, result: null);
                }
                return CreateErrroResult<byte[]>(result.Error);
            }
        }

        /// <summary>
        /// Download file to local cache folder.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="enableCache"></param>
        /// <param name="extension">File extension with dot</param>
        /// <returns></returns>
        public async UniTask<SpirareHttpClientResult<string>> DownloadToFileAsync(string url, bool enableCache = false, string extension = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                var ex = new ArgumentException();
                return CreateErrroResult<string>(ex);
            }

            if (IsFileUrl(url))
            {
                var ex = new ArgumentException();
                return CreateErrroResult<string>(ex);
            }

            if (enableCache)
            {
                var cacheResult = await GetCacheFileAsync(url, extension);
                if (cacheResult.Success)
                {
                    return CreateSuccessResult(cacheResult.Filename);
                }
            }

            var downloadTaskSource = enableCache ? SetCacheDownloadTaskSource(url) : null;

            var filepath = GetCacheFilePath(url, extension);
            var downloadHandler = new DownloadHandlerFile(filepath);

            var result = await SendGetRequestAsync(url, downloadHandler);
            if (result.Success)
            {
                result.Data.Dispose();

                if (enableCache)
                {
                    await SaveCacheMetaFile(url, filepath);

                    CompleteCacheDownloadTaskSouce(url, downloadTaskSource, filepath);
                }
                return CreateSuccessResult(filepath);
            }
            else
            {
                if (enableCache)
                {
                    CompleteCacheDownloadTaskSouce(url, downloadTaskSource, result: null);
                }
                return CreateErrroResult<string>(result.Error);
            }
        }

        public void ClearCache()
        {
            ClearFolder(cacheFolderPath);
        }

        private UniTaskCompletionSource<string> SetCacheDownloadTaskSource(string url)
        {
            var downloadTaskSource = new UniTaskCompletionSource<string>();
            cacheDownloadTaskDictionary[url] = downloadTaskSource.Task;
            return downloadTaskSource;
        }

        private void CompleteCacheDownloadTaskSouce(string url, UniTaskCompletionSource<string> downloadTaskSource, string result)
        {
            downloadTaskSource?.TrySetResult(result);
            cacheDownloadTaskDictionary.TryRemove(url, out _);
        }

        private async UniTask<SpirareHttpClientResult<UnityWebRequest>> SendGetRequestAsync(string url, DownloadHandler downloadHandler)
        {
            await semaphore.WaitAsync();

            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);
                request.downloadHandler = downloadHandler;
                var webRequest = await request.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return CreateSuccessResult(request);
                }
                else
                {
                    var ex = new Exception(webRequest.error);
                    request.Dispose();
                    return CreateErrroResult<UnityWebRequest>(ex);
                }
            }
            catch (Exception ex)
            {
                request?.Dispose();
                return CreateErrroResult<UnityWebRequest>(ex);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static string ConvertFileScemeUrlToFilePath(string url)
        {
            string filepath = url;
            if (url.StartsWith("file://"))
            {
                filepath = url.Substring("file://".Length);
            }

            // The URL might need to be decoded
            // filepath = Uri.UnescapeDataString(filepath);
            return filepath;
        }

        private async UniTask<SpirareHttpClientResult<byte[]>> LoadLocalFile(string url)
        {
            try
            {
                var filepath = ConvertFileScemeUrlToFilePath(url);
                using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, (int)stream.Length);

                return CreateSuccessResult(fileBytes);
            }
            catch (Exception ex)
            {
                return CreateErrroResult<byte[]>(ex);
            }
        }

        private string GetCacheFilePath(string url, string extension = "")
        {
            var hash = GetCacheFileHash(url);

            var index = 0;
            while (true)
            {
                var filename = $"{hash}-{index}{extension}";
                var pathCandidate = Path.Combine(cacheFolderPath, filename);

                if (File.Exists(pathCandidate))
                {
                    index += 1;
                }
                else
                {
                    return pathCandidate;
                }
            }
        }

        private string GetCacheFileHash(string url)
        {
            var hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(url));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hash;
        }

        private async UniTask<bool> SaveCacheMetaFile(string url, string filepath)
        {
            var metaFilePath = $"{filepath}.meta";
            var data = Encoding.UTF8.GetBytes(url);
            try
            {
                using (var fs = File.Open(metaFilePath, FileMode.CreateNew))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        private async UniTask<string> SaveCacheAsync(string url, byte[] data)
        {
            try
            {
                Directory.CreateDirectory(cacheFolderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }

            var cacheFilePath = GetCacheFilePath(url);

            try
            {
                using (var fs = File.Open(cacheFilePath, FileMode.CreateNew))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }

                await SaveCacheMetaFile(url, cacheFilePath);
                return cacheFilePath;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        private async UniTask<(bool Success, string Filename)> GetCacheFileAsync(string url, string extension = "")
        {
            var hash = GetCacheFileHash(url);

            foreach (var filePath in Directory.EnumerateFiles(cacheFolderPath, $"{hash}-*{extension}", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var metaFilePath = $"{filePath}.meta";
                    if (File.Exists(metaFilePath))
                    {
                        var reader = File.OpenText(metaFilePath);
                        var metaText = await reader.ReadLineAsync();
                        if (url == metaText)
                        {
                            return (true, filePath);
                        }
                    }
                }
                catch { }
            }

            if (cacheDownloadTaskDictionary.TryGetValue(url, out var downloadTask))
            {
                // Wait for download task
                var downloadedCachePath = await downloadTask;
                if (string.IsNullOrEmpty(downloadedCachePath) == false)
                {
                    return (true, downloadedCachePath);
                }
            }

            return (false, null);
        }

        private async UniTask<(bool Success, byte[] Data)> GetCacheDataAsync(string url)
        {
            var (success, filename) = await GetCacheFileAsync(url);
            if (success)
            {
                return await ReadFileAsync(filename);
            }
            else
            {
                return (false, Array.Empty<byte>());
            }
        }

        private static async UniTask<(bool Success, byte[] Data)> ReadFileAsync(string filepath)
        {
            try
            {
                using (var fs = File.OpenRead(filepath))
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

        private static void CreateDirectory(string folderPath)
        {
            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
