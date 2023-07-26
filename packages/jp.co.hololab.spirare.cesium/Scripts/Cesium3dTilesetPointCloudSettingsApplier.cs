using CesiumForUnity;
using System;
using System.Reflection;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class Cesium3dTilesetPointCloudSettingsApplier : MonoBehaviour
    {
        [SerializeField]
        private PointCloudRenderSettings pointCloudRenderSettings;

        private Cesium3DTileset cesium3DTileset;
        private PropertyInfo pointSizeProperty;

        private void Awake()
        {
            cesium3DTileset = GetComponent<Cesium3DTileset>();

            var pointCloudShading = cesium3DTileset.pointCloudShading;
            var pointCloudShadingType = pointCloudShading.GetType();
            pointSizeProperty = pointCloudShadingType.GetProperty("pointSize", BindingFlags.Public | BindingFlags.Instance);

            if (pointCloudRenderSettings != null)
            {
                pointCloudRenderSettings.OnPointSizeChanged += OnPointSizeChanged;
            }

            ApplySettings();
        }

        private void OnPointSizeChanged(float pointSize)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (pointSizeProperty == null)
            {
                return;
            }

            var pointCloudShading = cesium3DTileset.pointCloudShading;

            if (pointCloudRenderSettings == null || pointCloudRenderSettings.PointSize == 0)
            {
                pointCloudShading.attenuation = false;
            }
            else
            {
                pointCloudShading.attenuation = true;

                // Use reflection to set pointSize because pointSize proerty exists in the customized version.
                pointSizeProperty.SetValue(pointCloudShading, pointCloudRenderSettings.PointSize);
            }
        }
    }
}
