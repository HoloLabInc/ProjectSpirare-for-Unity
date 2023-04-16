#if UNITY_EDITOR && UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace HoloLab.Spirare.Browser.ARFoundation.Editor
{
    public class IOSDisableBitcodePostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == UnityEditor.BuildTargetGroup.iOS)
            {
                var projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
                var pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);

                var mainTargetGuid = pbxProject.GetUnityMainTargetGuid();
                pbxProject.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");

                var frameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
                pbxProject.SetBuildProperty(frameworkTargetGuid, "ENABLE_BITCODE", "NO");

                pbxProject.WriteToFile(projectPath);
            }
        }
    }
}
#endif
