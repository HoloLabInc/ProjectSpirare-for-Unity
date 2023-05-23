#if UNITY_EDITOR && UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace HoloLab.Spirare.Browser.ARFoundation.Editor
{
    public class IOSLocalNetworkUsagePostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == UnityEditor.BuildTargetGroup.iOS)
            {
                var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                plist.root.SetString("Privacy - Local Network Usage Description", "local network");

                plist.WriteToFile(plistPath);
            }
        }
    }
}
#endif
