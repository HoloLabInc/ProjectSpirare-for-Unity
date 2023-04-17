using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HoloLab.PositioningTools.CoordinateSystem;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.SpirareBrowser
{
    public class ReferencePointSettingsUI : MonoBehaviour
    {
        [SerializeField]
        private WorldCoordinateBinderWithLocationService worldCoordinateBinderWithLocationService = null;

        [SerializeField]
        private Button bindButton = null;

        [SerializeField]
        private Button unbindButton = null;

        [SerializeField]
        private Text bindingInfoText = null;

        [SerializeField]
        private ReferencePointType referencePointType = ReferencePointType.MainReferencePoint;

        private enum ReferencePointType
        {
            MainReferencePoint = 0,
            SubReferencePoint
        }

        private void Awake()
        {
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
        }

        private void OnBound(WorldBinding worldBinding)
        {
            var geodeticPose = worldBinding.GeodeticPose;
            var geodeticPosition = geodeticPose.GeodeticPosition;

            var builder = new StringBuilder();
            builder.AppendLine($"Latitude: {geodeticPosition.Latitude}, Longitude: {geodeticPosition.Longitude}");
            builder.AppendLine($"EllipsoidalHeight: {geodeticPosition.EllipsoidalHeight}, EnuRotation: {geodeticPose.EnuRotation.eulerAngles}");
            bindingInfoText.text = builder.ToString();
        }

        private void OnDestroy()
        {
            bindButton.onClick.RemoveListener(BindButton_OnClick);
            unbindButton.onClick.RemoveListener(UnbindButton_OnClick);
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