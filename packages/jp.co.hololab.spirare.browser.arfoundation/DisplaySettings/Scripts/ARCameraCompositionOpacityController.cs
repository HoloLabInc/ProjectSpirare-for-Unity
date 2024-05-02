using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ARCAMERACOMPOSITION_PRESENT
using HoloLab.ARCameraComposition;
#endif

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class ARCameraCompositionOpacityController : MonoBehaviour
    {
#if ARCAMERACOMPOSITION_PRESENT
        private ARCameraCompositionOpacityConfigurator configurator;
#endif

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

#if ARCAMERACOMPOSITION_PRESENT
        private void Start()
        {
            configurator = GetComponent<ARCameraCompositionOpacityConfigurator>();

            SetOpacity(displaySettingsState.Opacity);

            displaySettingsState.OnOpacityChanged += DisplaySettingsState_OnOpacityChanged;
        }

        private void DisplaySettingsState_OnOpacityChanged(float opacity)
        {
            SetOpacity(opacity);
        }

        private void SetOpacity(float opacity)
        {
            configurator.SetOpacity(opacity);
        }
#else
        private void Awake()
        {
            Debug.LogWarning("ARCameraComposition-Unity should be imported in the project");
        }
#endif
    }
}

