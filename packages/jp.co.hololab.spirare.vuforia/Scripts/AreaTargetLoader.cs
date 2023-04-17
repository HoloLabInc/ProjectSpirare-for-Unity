using HoloLab.PositioningTools.Vuforia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if VUFORIA_PRESENT
using Vuforia;
#endif

namespace HoloLab.Spirare.Vuforia
{
    public class AreaTargetLoader : MonoBehaviour
    {
        private string AreaTargetDataRootPath
        {
            get => Path.Combine(Application.persistentDataPath, "AreaTargetData");
        }

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
            var spaceBinderWithVuforiaAreaTarget = GameObject.FindObjectOfType<SpaceBinderWithVuforiaAreaTarget>();

            if (!Directory.Exists(AreaTargetDataRootPath))
            {
                Directory.CreateDirectory(AreaTargetDataRootPath);
            }

            Debug.Log($"Area target data root path: {AreaTargetDataRootPath}");

            var mapFiles = Directory.EnumerateFiles(AreaTargetDataRootPath, "*.xml", SearchOption.AllDirectories);

            foreach (var mapFile in mapFiles)
            {
                try
                {
                    spaceBinderWithVuforiaAreaTarget.LoadAreaTarget(mapFile);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }
    }
}
