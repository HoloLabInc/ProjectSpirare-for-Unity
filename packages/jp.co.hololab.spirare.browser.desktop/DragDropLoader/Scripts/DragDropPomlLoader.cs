using B83.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Desktop
{
    [RequireComponent(typeof(OnetimeLocalPomlLoader))]
    public class DragDropPomlLoader : MonoBehaviour
    {
        private OnetimeLocalPomlLoader onetimeLocalPomlLoader;

        private void Awake()
        {
            onetimeLocalPomlLoader = GetComponent<OnetimeLocalPomlLoader>();
        }

        private void OnEnable()
        {
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnDroppedFiles;
        }

        private void OnDisable()
        {
            UnityDragAndDropHook.UninstallHook();
        }

        private async void OnDroppedFiles(List<string> filePathList, POINT point)
        {
            foreach (var filePath in filePathList)
            {
                var attr = File.GetAttributes(filePath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    await LoadPomlInDirectoryAsync(filePath);
                }
                else
                {
                    await LoadFileAsync(filePath);
                }
            }
        }

        private async Task LoadPomlInDirectoryAsync(string directoryPath)
        {
            var pomlFiles = SearchAllFiles(directoryPath, "poml");
            foreach (var pomlFile in pomlFiles)
            {
                await LoadPomlAsync(pomlFile);
            }

            var pomlZipFiles = SearchAllFiles(directoryPath, "poml.zip");
            foreach (var pomlZipFile in pomlZipFiles)
            {
                await LoadPomlAsync(pomlZipFile);
            }
        }

        private async Task LoadFileAsync(string filePath)
        {
            var lowerFilePath = filePath.ToLower();
            if (Path.GetFileName(lowerFilePath) == "tileset.json")
            {
                await Load3DTilesAsync(filePath);
            }
            else
            {
                await LoadPomlAsync(filePath);
            }
        }

        private async Task LoadPomlAsync(string filePath)
        {
            var lowerFilePath = filePath.ToLower();
            if (lowerFilePath.EndsWith(".poml"))
            {
                await onetimeLocalPomlLoader.LoadPomlAsync(filePath);
            }
            else if (lowerFilePath.EndsWith(".zip"))
            {
                await onetimeLocalPomlLoader.LoadPomlZipAsync(filePath);
            }
        }

        private async Task Load3DTilesAsync(string filePath)
        {
            var lowerFilePath = filePath.ToLower();
            if (lowerFilePath.EndsWith(".json"))
            {
                var poml = @$"
<poml>
  <scene>
    <cesium3dtiles src=""{filePath}""></cesium3dtiles>
  </scene>
</poml>";

                string tempPath = Path.GetTempPath();
                string tempFileName = Path.GetRandomFileName() + ".poml";
                string tempFilePath = Path.Combine(tempPath, tempFileName);

                try
                {
                    File.WriteAllText(tempFilePath, poml);
                    await LoadPomlAsync(tempFilePath);
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static string[] SearchAllFiles(string folderPath, string extension)
        {
            return Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);
        }

    }
}
