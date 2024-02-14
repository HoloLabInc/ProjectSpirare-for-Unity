#if UNITY_EDITOR && UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace HoloLab.Spirare.Browser.Immersal.Editor{
public  class IOSEmbedSwiftPostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == UnityEditor.BuildTargetGroup.iOS)
            {
                var projectPath =   PBXProject.GetPBXProjectPath(report.summary.outputPath);

                var project = new PBXProject();
                project.ReadFromFile(projectPath);

                var     mainTargetGuid =    project.GetUnityMainTargetGuid();
                var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

                    var key = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
               project.SetBuildProperty(frameworkTargetGuid, key, "NO");

               project.SetBuildProperty(mainTargetGuid, key, "YES");
               project.WriteToFile(projectPath);
            }
        }
    }
}
#endif
