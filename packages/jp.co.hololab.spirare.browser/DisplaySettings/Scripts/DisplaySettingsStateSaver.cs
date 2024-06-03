using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

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
        private const string StreetscapeGeometryOcclusionKey = "DisplaySettingsStateSaver_StreetscapeGeometryOcclusion";

        private void Awake()
        {
            LoadState();

            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
            displaySettingsState.OnOpacityChanged += DisplaySettingsState_OnOpacityChanged;
            displaySettingsState.OnFarClipChanged += DisplaySettingsState_OnFarClipChanged;
            displaySettingsState.OnOcclusionChanged += DisplaySettingsState_OnOcclusionChanged;
            displaySettingsState.OnStreetscapeGeometryOcclusionChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;
        }

        private void LoadState()
        {
            LoadIsMenuOpen();
            LoadOpacity();
            LoadFarClip();
            LoadOcclusion();
            LoadStreetscapeGeometryOcclusion();
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
            if (PlayerPrefsUtility.TryGetEnum(OcclusionKey, out OcclusionType occlusion))
            {
                displaySettingsState.Occlusion = occlusion;
            }
        }

        private void DisplaySettingsState_OnOcclusionChanged(OcclusionType occlusion)
        {
            PlayerPrefsUtility.SetEnum(OcclusionKey, occlusion);
        }

        private void LoadStreetscapeGeometryOcclusion()
        {
            if (PlayerPrefsUtility.TryGetEnum(StreetscapeGeometryOcclusionKey, out StreetscapeGeometryOcclusionType streetscapeGeometryOcclusionEnabled))
            {
                displaySettingsState.StreetscapeGeometryOcclusion = streetscapeGeometryOcclusionEnabled;
            }
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged(StreetscapeGeometryOcclusionType streetscapeGeometryOcclusion)
        {
            PlayerPrefsUtility.SetEnum(StreetscapeGeometryOcclusionKey, streetscapeGeometryOcclusion);
        }
    }
}

