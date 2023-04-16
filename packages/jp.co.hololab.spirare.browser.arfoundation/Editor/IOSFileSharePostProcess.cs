// Copyright (c) 2021 HoloLab Inc. All rights reserved.

#if UNITY_EDITOR && UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace HoloLab.Toolkit.Media.ARPlayback
{
    public class IOSFileSharePostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == UnityEditor.BuildTargetGroup.iOS)
            {
                var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                // Enable file sharing using iTunes.
                plist.root.SetBoolean("UIFileSharingEnabled", true);

                // Enable the display of the save folder in the Files app.
                plist.root.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

                plist.WriteToFile(plistPath);
            }
        }
    }
}
#endif
