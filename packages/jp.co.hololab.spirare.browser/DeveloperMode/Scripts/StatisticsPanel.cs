using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class StatisticsPanel : MonoBehaviour
    {
        [SerializeField]
        private DeveloperModeState developerModeState;

        private void Start()
        {
            ApplyDeveloerModeEnabled(developerModeState.Enabled);
            developerModeState.OnEnabledChanged += DeveloperModeState_OnEnabledChanged;
        }

        private void DeveloperModeState_OnEnabledChanged(bool enabled)
        {
            ApplyDeveloerModeEnabled(enabled);
        }

        private void ApplyDeveloerModeEnabled(bool enabled)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(enabled);
            }
        }
    }
}
