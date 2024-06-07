using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.Spirare.Browser.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.SpirareBrowser
{
    public class ReferencePointSettingsUI : MonoBehaviour
    {
        [SerializeField]
        private WorldCoordinateBinderWithLocationService worldCoordinateBinderWithLocationService = null;

        [SerializeField]
        private ToggleButton autoAlignToggle = null;

        [SerializeField]
        private Button bindButton = null;

        [SerializeField]
        private Button unbindButton = null;

        [SerializeField]
        private TMP_Text bindingInfoText = null;

        [SerializeField]
        private ReferencePointType referencePointType = ReferencePointType.MainReferencePoint;

        private enum ReferencePointType
        {
            MainReferencePoint = 0,
            SubReferencePoint
        }

        private void Start()
        {
            //autoAlignToggle.onValueChanged.AddListener(AutoAlignToggle_OnValueChanged);
            autoAlignToggle.OnToggle += AutoAlignToggle_OnToggle;
            bindButton.onClick.AddListener(BindButton_OnClick);
            unbindButton.onClick.AddListener(UnbindButton_OnClick);

            switch (referencePointType)
            {
                case ReferencePointType.MainReferencePoint:
                    worldCoordinateBinderWithLocationService.OnReferencePointBound += OnBound;
                    break;
                case ReferencePointType.SubReferencePoint:
                    worldCoordinateBinderWithLocationService.OnSubReferencePointBound += OnBound;
                    break;
            }

            AutoAlignToggle_OnValueChanged(autoAlignToggle.IsOn);
        }

        private void AutoAlignToggle_OnToggle(bool value)
        {
            AutoAlignToggle_OnValueChanged(value);
        }

        private void OnBound(WorldBinding worldBinding)
        {
            var geodeticPose = worldBinding.GeodeticPose;
            var geodeticPosition = geodeticPose.GeodeticPosition;

            var heading = geodeticPose.EnuRotation.eulerAngles.y;

            var builder = new StringBuilder();
            builder.AppendLine($"lat: {geodeticPosition.Latitude:f9}, lon: {geodeticPosition.Longitude:f9}");
            builder.AppendLine($"height: {geodeticPosition.EllipsoidalHeight:f2}, heading: {heading:f1}");
            bindingInfoText.text = builder.ToString();
        }

        private void OnDestroy()
        {
            bindButton.onClick.RemoveListener(BindButton_OnClick);
            unbindButton.onClick.RemoveListener(UnbindButton_OnClick);
        }

        private void AutoAlignToggle_OnValueChanged(bool autoAlignEnabled)
        {
            if (bindButton != null)
            {
                bindButton.interactable = !autoAlignEnabled;
            }
            if (unbindButton != null)
            {
                unbindButton.interactable = !autoAlignEnabled;
            }

            if (worldCoordinateBinderWithLocationService != null)
            {
                worldCoordinateBinderWithLocationService.AutoUpdateReferencePoint = autoAlignEnabled;
            }
        }

        private void BindButton_OnClick()
        {
            switch (referencePointType)
            {
                case ReferencePointType.MainReferencePoint:
                    worldCoordinateBinderWithLocationService.BindReferencePoint();
                    return;
                case ReferencePointType.SubReferencePoint:
                    worldCoordinateBinderWithLocationService.BindSubReferencePoint();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UnbindButton_OnClick()
        {
            bindingInfoText.text = "";
            switch (referencePointType)
            {
                case ReferencePointType.MainReferencePoint:
                    worldCoordinateBinderWithLocationService.UnbindReferencePoint();
                    return;
                case ReferencePointType.SubReferencePoint:
                    worldCoordinateBinderWithLocationService.UnbindSubReferencePoint();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}