using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloLab.PositioningTools.Vuforia;
#if VUFORIA_PRESENT
using Vuforia;
#endif

namespace HoloLab.Spirare.Vuforia
{
    public class VuforiaStatusLog : MonoBehaviour
    {
        [SerializeField]
        private SpaceBinderWithVuforiaAreaTarget spaceBinderWithVuforiaAreaTarget;

        [SerializeField]
        private Text logText;

        private void Awake()
        {
            logText.text = null;
#if VUFORIA_PRESENT
            spaceBinderWithVuforiaAreaTarget.OnTrackingStatusChanged += OnTrackingStatusChanged;
#endif
        }

#if VUFORIA_PRESENT
        private void OnTrackingStatusChanged(ObserverBehaviour observerBehaviour, TargetStatus targetStatus)
        {
            var targetName = observerBehaviour.TargetName;
            var text = $"{targetName}\n{targetStatus.Status}, {targetStatus.StatusInfo}";
            logText.text = text;
        }
#endif
    }
}
