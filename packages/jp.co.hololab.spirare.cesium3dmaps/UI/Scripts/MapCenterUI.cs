using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class MapCenterUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;

        private CesiumRectangleMap cesiumRectangleMap;

        private void Start()
        {
            cesiumRectangleMap = FindObjectOfType<CesiumRectangleMap>();

            if (cesiumRectangleMap != null)
            {
                CesiumRectangleMap_OnCenterChanged(cesiumRectangleMap.Center);
                cesiumRectangleMap.OnCenterChanged += CesiumRectangleMap_OnCenterChanged;
            }

            inputField.onEndEdit.AddListener(InputField_OnValueChanged);
        }

        private void CesiumRectangleMap_OnCenterChanged(GeodeticPosition position)
        {
            inputField.text = $"{position.Latitude}, {position.Longitude}, {position.EllipsoidalHeight}";
        }

        private void InputField_OnValueChanged(string text)
        {
            var separator = new char[] { ',', ' ', '(', ')' };
            var tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                return;
            }

            if (double.TryParse(tokens[0], out var latitude) && double.TryParse(tokens[1], out var longitude))
            {
                var center = cesiumRectangleMap.Center;
                cesiumRectangleMap.Center = new GeodeticPosition(latitude, longitude, center.EllipsoidalHeight);
            }

            if (tokens.Length >= 3)
            {
                if (float.TryParse(tokens[2], out var height))
                {
                    var center = cesiumRectangleMap.Center;
                    cesiumRectangleMap.Center = new GeodeticPosition(center.Latitude, center.Longitude, height);
                }
            }
        }
    }
}
