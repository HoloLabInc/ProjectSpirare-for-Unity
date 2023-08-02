using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CesiumGeoreference))]
[RequireComponent(typeof(WorldCoordinateOrigin))]
public class WorldCoordinateOriginForCesiumGeoreference : MonoBehaviour
{
    private CesiumGeoreference cesiumGeoreference;
    private WorldCoordinateOrigin worldCoordinateOrigin;

    private CoordinateManager coordinateManager;

    private WorldBinding latestWorldBinding;

    private void Start()
    {
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

        latestWorldBinding = worldBinding;

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
