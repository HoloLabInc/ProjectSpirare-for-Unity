using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(CesiumGeoreference))]
    [RequireComponent(typeof(WorldCoordinateOrigin))]
    [ExecuteAlways]
    public class WorldCoordinateOriginForCesiumGeoreference : MonoBehaviour
    {
        private CesiumGeoreference cesiumGeoreference;
        private WorldCoordinateOrigin worldCoordinateOrigin;

        private CoordinateManager coordinateManager;

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            cesiumGeoreference = GetComponent<CesiumGeoreference>();
            worldCoordinateOrigin = GetComponent<WorldCoordinateOrigin>();
            worldCoordinateOrigin.PositionSettingMode = WorldCoordinateOrigin.PositionSettingModeType.GeodeticPosition;

            coordinateManager = CoordinateManager.Instance;
            coordinateManager.OnCoordinatesBound += OnCoordinatesBound;
            gameObject.SetActive(false);

            if (coordinateManager.LatestWorldBinding != null)
            {
                OnCoordinatesBound(coordinateManager.LatestWorldBinding);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                BindCoordinatesInEditMode();
            }
#endif
        }

#if UNITY_EDITOR
        private void BindCoordinatesInEditMode()
        {
            WorldBinding worldBinding;

            var parentBinder = GetComponentInParent<WorldCoordinateBinder>();
            if (parentBinder != null)
            {
                worldBinding = parentBinder.TransformWorldBinding;
            }
            else if (coordinateManager == null)
            {
                coordinateManager = FindObjectOfType<CoordinateManager>();

                if (coordinateManager == null)
                {
                    return;
                }
                worldBinding = coordinateManager.WorldBindingInEditor;
            }
            else
            {
                return;
            }

            cesiumGeoreference = GetComponent<CesiumGeoreference>();
            worldCoordinateOrigin = GetComponent<WorldCoordinateOrigin>();
            worldCoordinateOrigin.PositionSettingMode = WorldCoordinateOrigin.PositionSettingModeType.GeodeticPosition;

            BindCoordinates(worldBinding);
        }
#endif

        private void OnCoordinatesBound(WorldBinding worldBinding)
        {
            BindCoordinates(worldBinding);
            gameObject.SetActive(true);
        }

        private void BindCoordinates(WorldBinding worldBinding)
        {
            if (worldBinding == null)
            {
                return;
            }

            var geodeticPose = worldBinding.GeodeticPose;
            var geodeticPosition = geodeticPose.GeodeticPosition;

            if (cesiumGeoreference != null)
            {
                if (cesiumGeoreference.isActiveAndEnabled == false)
                {
                    cesiumGeoreference.Initialize();
                }
                cesiumGeoreference.SetOriginLongitudeLatitudeHeight(geodeticPosition.Longitude, geodeticPosition.Latitude, geodeticPosition.EllipsoidalHeight);
            }

            if (worldCoordinateOrigin != null)
            {
                worldCoordinateOrigin.GeodeticPosition = geodeticPosition;
                worldCoordinateOrigin.EnuRotation = Quaternion.identity;
            }
        }
    }
}
