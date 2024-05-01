using HoloLab.Spirare.Browser.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryToggle : MonoBehaviour
    {
        [SerializeField]
        private GameObject streetscapeGeometryObject;

        [SerializeField]
        private ToggleButton toggleButton;

        private void Start()
        {
            ToggleButton_OnToggle(toggleButton.IsOn);
            toggleButton.OnToggle += ToggleButton_OnToggle;
        }

        private void ToggleButton_OnToggle(bool value)
        {
            streetscapeGeometryObject.SetActive(value);
        }
    }
}

