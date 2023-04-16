using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    [RequireComponent(typeof(PomlContentsManager))]
    public class LocalPomlContentLoader : MonoBehaviour
    {
        private PomlContentsManager contentManager;

        private const string localCeontentSaveDataKey = "LocalPomlContentLoader_LocalContentSaveData";

        private string LocalContentFolderPath
        {
            get
            {
                string rootPath;

#if UNITY_EDITOR
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                rootPath = Path.Combine(documentsPath, "SpirareBrowser");
#else
                rootPath = Application.persistentDataPath;
#endif

                var contentFolderName = "LocalContent";
                var path = Path.Combine(rootPath, contentFolderName);
                return path;
            }
        }

        private string ContentCacheFolderPath
        {
            get
            {
                return Path.Combine(Application.temporaryCachePath, "LocalContent");
            }
        }

        private void Awake()
        {
            contentManager = GetComponent<PomlContentsManager>();
        }

        private async void Start()
        {
            await LoadAll();
        }

        private async Task LoadAll()
        {
            var path = LocalContentFolderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Exclude from iCloud backup
#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif

#if UNITY_EDITOR
            Debug.Log($"Load content within {path}");
            Debug.Log($"Unzip zip files in {ContentCacheFolderPath}");
#endif

            // Load poml files within the content folder
            await LoadLocalPomlInDirectoryAsync(path);

            // Load poml.zip files within the content folder
            await LoadAllPomlZipAsync(path);
        }

        /// <summary>
        /// Load all poml files within the directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task LoadLocalPomlInDirectoryAsync(string path)
        {
            try
            {
                // Search all poml files
                var pomlFiles = SearchAllFiles(path, "poml");
                foreach (var pomlFile in pomlFiles)
                {
                    await contentManager.LoadLocalContentAsync(pomlFile);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Load all poml.zip files within the directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task LoadAllPomlZipAsync(string path)
        {
            var previousSaveData = LoadSaveData();
            var previousPomlZipHashDictionary = new Dictionary<string, string>();
            try
            {
                previousPomlZipHashDictionary = previousSaveData.PomlZipInfoList.ToDictionary(x => x.Path, x => x.Hash);
            }
            catch
            {
                // do nothing
            }

            var pomlZipInfoList = new List<PomlZipInfo>();

            var pomlZipFiles = SearchAllFiles(path, "zip");
            foreach (var pomlZipFilePath in pomlZipFiles)
            {
                if (!pomlZipFilePath.StartsWith(path))
                {
                    continue;
                }

                var relativePath = pomlZipFilePath.Substring(path.Length + 1);
                var destinationPath = Path.Combine(ContentCacheFolderPath, relativePath);

                // Calculate the hash of the zip file
                if (TryComputeHash(pomlZipFilePath, out var hash) == false)
                {
                    continue;
                }

                // Extract the Zip if the destination folder does not exist, or the previous hash does not match
                if (Directory.Exists(destinationPath) == false
                    || previousPomlZipHashDictionary.TryGetValue(destinationPath, out var previousHash) == false
                    || hash != previousHash)
                {
                    TryDeleteDirectory(destinationPath, true);

                    try
                    {
                        ZipFile.ExtractToDirectory(pomlZipFilePath, destinationPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        continue;
                    }
                }

                pomlZipInfoList.Add(new PomlZipInfo()
                {
                    Path = destinationPath,
                    Hash = hash,
                });

                await LoadLocalPomlInDirectoryAsync(destinationPath);
            }

            // Save hash values
            var saveData = new LocalContentSaveData
            {
                PomlZipInfoList = pomlZipInfoList
            };
            SaveLocalContentData(saveData);
        }

        private LocalContentSaveData LoadSaveData()
        {
            try
            {
                var saveDataString = PlayerPrefs.GetString(localCeontentSaveDataKey);
                var saveData = JsonUtility.FromJson<LocalContentSaveData>(saveDataString);
                return saveData;
            }
            catch
            {
                return new LocalContentSaveData();
            }
        }

        private void SaveLocalContentData(LocalContentSaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(localCeontentSaveDataKey, json);
            PlayerPrefs.Save();
        }

        [Serializable]
        private class LocalContentSaveData
        {
            public List<PomlZipInfo> PomlZipInfoList = new List<PomlZipInfo>();
        }

        [Serializable]
        private class PomlZipInfo
        {
            public string Path;
            public string Hash;
        }

        private static bool TryComputeHash(string path, out string hash)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var hashBytes = md5.ComputeHash(stream);

                    var stringBuilder = new StringBuilder();

                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        stringBuilder.Append(hashBytes[i].ToString("x2"));
                    }

                    hash = stringBuilder.ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                hash = null;
                return false;
            }
        }

        private static bool TryDeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private static string[] SearchAllFiles(string folderPath, string extension)
        {
            return Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);
        }
    }
}
