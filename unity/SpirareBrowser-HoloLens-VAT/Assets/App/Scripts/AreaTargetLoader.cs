using HoloLab.PositioningTools.Vuforia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if VUFORIA_PRESENT
using Vuforia;
#endif

namespace HoloLab.Spirare.Browser.Vuforia
{
    public class AreaTargetLoader : MonoBehaviour
    {
        private void Awake()
        {
#if VUFORIA_PRESENT
            VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
#endif
        }

        private void OnVuforiaStarted()
        {
#if VUFORIA_PRESENT
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
#endif
            LoadAreaTarget();
        }

        private void LoadAreaTarget()
        {
            var areaTargetDataRootPath = GetAreaTargetDataRootPath();

            // In Unity 2021, access to the Documents folder on HoloLens did not work,
            // so the method has been changed from copying from the Documents folder to directly placing files under the PersistentDataPath.

            // var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // var documentsFolderDataRootPath = Path.Combine(documentsFolderPath, "SpirareBrowser", "AreaTargetData");
            // SyncFolder(documentsFolderDataRootPath, areaTargetDataRootPath);

            LoadAreaTargetMaps(areaTargetDataRootPath);
        }

        private string GetAreaTargetDataRootPath()
        {
#if WINDOWS_UWP
            // It was not possible to directly load map data from the document folder.
            //
            // Vuforia.ObserverNotCreatedException`1[Vuforia.VuAreaTargetCreationError]: Failed to create AreaTargetObserver: DATABASE_LOAD_ERROR.
            //
            // var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // return Path.Combine(documentsFolderPath, "SpirareBrowser", "AreaTargetData");

            return Path.Combine(Application.persistentDataPath, "AreaTargetData");
#else
            var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsFolderPath, "SpirareBrowser", "AreaTargetData");
#endif
        }

        private void LoadAreaTargetMaps(string rootPath)
        {
            var spaceBinderWithVuforiaAreaTarget = GameObject.FindObjectOfType<SpaceBinderWithVuforiaAreaTarget>();

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            Debug.Log($"Area target data root path: {rootPath}");

            var mapFiles = Directory.EnumerateFiles(rootPath, "*.xml", SearchOption.AllDirectories);
            foreach (var mapFile in mapFiles)
            {
                try
                {
                    Debug.Log($"Load area target data: {mapFile}");
                    spaceBinderWithVuforiaAreaTarget.LoadAreaTarget(mapFile);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        private void SyncFolder(string sourceFolder, string destinationFolder)
        {
            try
            {
                if (!Directory.Exists(sourceFolder))
                {
                    Directory.CreateDirectory(sourceFolder);
                }

                if (Directory.Exists(destinationFolder))
                {
                    Directory.Delete(destinationFolder, true);
                }

                var copySubDirs = true;
                DirectoryCopy(sourceFolder, destinationFolder, copySubDirs);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Copy directory
        /// https://docs.microsoft.com/ja-jp/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                try
                {
                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    try
                    {
                        string tempPath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}
