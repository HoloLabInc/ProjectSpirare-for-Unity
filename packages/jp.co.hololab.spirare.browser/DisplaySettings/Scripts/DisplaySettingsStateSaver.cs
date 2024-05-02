using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class DisplaySettingsStateSaver : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private const string IsMenuOpenKey = "DisplaySettingsStateSaver_IsMenuOpen";
        private const string OpacityKey = "DisplaySettingsStateSaver_Opacity";
        private const string FarClipKey = "DisplaySettingsStateSaver_FarClip";
        private const string OcclusionKey = "DisplaySettingsStateSaver_Occlusion";

        private void Awake()
        {
            LoadState();

            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
            displaySettingsState.OnOpacityChanged += DisplaySettingsState_OnOpacityChanged;
            displaySettingsState.OnFarClipChanged += DisplaySettingsState_OnFarClipChanged;
            displaySettingsState.OnOcclusionChanged += DisplaySettingsState_OnOcclusionChanged;
        }

        private void LoadState()
        {
            LoadIsMenuOpen();
            LoadOpacity();
            LoadFarClip();
            LoadOcclusion();
        }

        private void LoadIsMenuOpen()
        {
            if (PlayerPrefsUtility.TryGetBoolean(IsMenuOpenKey, out var isMenuOpen))
            {
                displaySettingsState.IsMenuOpen = isMenuOpen;
            }
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged(bool isMenuOpen)
        {
            PlayerPrefsUtility.SetBoolean(IsMenuOpenKey, isMenuOpen);
        }

        private void LoadOpacity()
        {
            if (PlayerPrefsUtility.TryGetFloat(OpacityKey, out var opacity))
            {
                displaySettingsState.Opacity = opacity;
            }
        }

        private void DisplaySettingsState_OnOpacityChanged(float opacity)
        {
            PlayerPrefs.SetFloat(OpacityKey, opacity);
        }

        private void LoadFarClip()
        {
            if (PlayerPrefsUtility.TryGetFloat(FarClipKey, out var farClip))
            {
                displaySettingsState.FarClip = farClip;
            }
        }

        private void DisplaySettingsState_OnFarClipChanged(float farClip)
        {
            PlayerPrefs.SetFloat(FarClipKey, farClip);
        }

        private void LoadOcclusion()
        {
            if (PlayerPrefsUtility.TryGetEnum(OcclusionKey, out DisplaySettingsState.OcclusionType occlusion))
            {
                displaySettingsState.Occlusion = occlusion;
            }
        }

        private void DisplaySettingsState_OnOcclusionChanged(DisplaySettingsState.OcclusionType occlusion)
        {
            PlayerPrefsUtility.SetEnum(OcclusionKey, occlusion);
        }
    }
}

