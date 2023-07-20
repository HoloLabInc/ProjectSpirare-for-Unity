using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Desktop
{
    [RequireComponent(typeof(PomlContentsManager))]
    public class OnetimeLocalPomlLoader : MonoBehaviour
    {
        private PomlContentsManager contentManager;

        private string ExtractFolderPath
        {
            get
            {
                return Path.Combine(Application.temporaryCachePath, "OnetimeLocalContent");
            }
        }

        private void Awake()
        {
            contentManager = GetComponent<PomlContentsManager>();
        }

        public async Task LoadPomlAsync(string filepath)
        {
            await contentManager.LoadLocalContentAsync(filepath);
        }

        public async Task LoadPomlZipAsync(string filepath)
        {
            if (Directory.Exists(ExtractFolderPath) == false)
            {
                Directory.CreateDirectory(ExtractFolderPath);
            }

            var extractFolderPath = CreateTemporaryFolderPath(ExtractFolderPath);
            try
            {
                ZipFile.ExtractToDirectory(filepath, extractFolderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            await LoadLocalPomlInDirectoryAsync(extractFolderPath);
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

        private static string CreateTemporaryFolderPath(string parentFolderPath)
        {
            while (true)
            {
                var randomName = Guid.NewGuid().ToString();
                var path = Path.Combine(parentFolderPath, randomName);
                if (Directory.Exists(path))
                {
                    continue;
                }

                return path;
            }
        }

        private static string[] SearchAllFiles(string folderPath, string extension)
        {
            return Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);
        }
    }
}
