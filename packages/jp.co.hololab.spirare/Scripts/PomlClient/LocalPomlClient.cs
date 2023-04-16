using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class LocalPomlClient : PomlClientBase
    {
        [SerializeField]
        [Tooltip("Absolute file path, or relative path from the Assets folder")]
        private string filePath;

        private async void Awake()
        {
            if (string.IsNullOrEmpty(filePath) == false)
            {
                var fullPath = GetFullPath(filePath);
                await LoadAsync(fullPath);
            }
        }

        private static string GetFullPath(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }
            else
            {
                return Path.Combine(Application.dataPath, filePath);
            }
        }

        protected override async Task<string> GetContentXml(string path)
        {
            return await Task.Run(() => File.ReadAllText(path));
        }
    }
}
