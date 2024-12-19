#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;

namespace HoloLab.Spirare.Browser.VisionOS3DMaps.Editor
{
    public class CesiumPostProcessBuildForVisionOS
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.VisionOS)
            {
                return;
            }

            if (TryFindCesiumForUnityPackage(out var packagePath) == false)
            {
                Debug.LogError("Cesium for Unity package not found.");
                return;
            }

            var pluginPath = Path.Combine(packagePath, "Plugins", "iOS");
            var librariesPath = Path.Combine(pathToBuiltProject, "Libraries", "Cesium");

            CopyFolder(pluginPath, librariesPath, ".a");

            var projPath = Path.Combine(pathToBuiltProject, "Unity-VisionOS.xcodeproj/project.pbxproj");
            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            var targetGuid = proj.GetUnityFrameworkTargetGuid();

            var libraryFiles = Directory.GetFiles(librariesPath, "*.a", SearchOption.AllDirectories);
            if (libraryFiles.Any(x => Path.GetFileName(x) == "libjpeg.a") && libraryFiles.Any(x => Path.GetFileName(x) == "libturbojpeg.a"))
            {
                libraryFiles = libraryFiles.Where(x => Path.GetFileName(x) != "libjpeg.a").ToArray();
            }
            if (libraryFiles.Any(x => Path.GetFileName(x) == "libwebpdecoder.a") && libraryFiles.Any(x => Path.GetFileName(x) == "libwebp.a"))
            {
                libraryFiles = libraryFiles.Where(x => Path.GetFileName(x) != "libwebpdecoder.a").ToArray();
            }

            foreach (var libraryFile in libraryFiles)
            {
                var relativePath = Path.GetRelativePath(pathToBuiltProject, libraryFile);
                var fileGuid = proj.AddFile(relativePath, relativePath, PBXSourceTree.Source);
                proj.AddFileToBuild(targetGuid, fileGuid);
            }

            proj.AddBuildProperty(targetGuid, "LIBRARY_SEARCH_PATHS", "$(PROJECT_DIR)/Libraries/Cesium");
            proj.AddBuildProperty(targetGuid, "LIBRARY_SEARCH_PATHS", "$(PROJECT_DIR)/Libraries/Cesium/lib");

            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static bool TryFindCesiumForUnityPackage(out string packagePath)
        {
            var listRequest = Client.List();
            while (listRequest.IsCompleted == false)
            {
                System.Threading.Thread.Sleep(10);
            }

            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name == "com.cesium.unity")
                    {
                        packagePath = package.resolvedPath;
                        return true;
                    }
                }
            }

            packagePath = null;
            return false;
        }

        private static void CopyFolder(string sourceFolder, string destFolder, string extension = null)
        {
            if (!Directory.Exists(sourceFolder))
            {
                return;
            }

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            var dir = new DirectoryInfo(sourceFolder);

            // Copy subfolders
            var subdirs = dir.GetDirectories();
            foreach (var subdir in subdirs)
            {
                var destPath = Path.Combine(destFolder, subdir.Name);
                CopyFolder(subdir.FullName, destPath, extension);
            }

            // Copy files
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                if (extension != null && file.Extension != extension)
                {
                    continue;
                }

                var destPath = Path.Combine(destFolder, file.Name);
                try
                {
                    file.CopyTo(destPath, false);
                }
                catch { }
            }
        }
    }
}
#endif
