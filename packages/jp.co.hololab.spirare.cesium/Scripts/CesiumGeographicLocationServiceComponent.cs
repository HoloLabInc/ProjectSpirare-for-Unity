using CesiumForUnity;
using HoloLab.PositioningTools;
using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class CesiumGeographicLocationServiceComponent : MonoBehaviour, IGeographicLocationService, ICardinalDirectionService
    {
        [SerializeField]
        private Transform targetTransform = null;

        CesiumGeoreference cesiumGeoreference;

        private bool serviceEnabled;

        public event Action<GeographicLocation> OnLocationUpdated;
        public event Action<CardinalDirection> OnDirectionUpdated;

        private void Awake()
        {
            if (targetTransform == null)
            {
                targetTransform = Camera.main.transform;
            }

#if UNITY_2021_3_18_OR_NEWER
            cesiumGeoreference = FindFirstObjectByType<CesiumGeoreference>();
#else
            cesiumGeoreference = FindObjectOfType<CesiumGeoreference>();
#endif
        }

        private void Update()
        {
            if (serviceEnabled)
            {
                NotifyLocation();
            }
        }

        public void StartService()
        {
            _ = StartServiceAsync();
        }

        public Task<(bool ok, Exception exception)> StartServiceAsync()
        {
            if (cesiumGeoreference == null)
            {
                return Task.FromResult<(bool, Exception)>((false, new InvalidOperationException("CesiumGeoreference not found")));
            }
            else
            {
                serviceEnabled = true;
                return Task.FromResult<(bool, Exception)>((true, null));
            }
        }

        public void StopService()
        {
            _ = StopServiceAsync();
        }

        public Task<(bool ok, Exception exception)> StopServiceAsync()
        {
            serviceEnabled = false;
            return Task.FromResult<(bool, Exception)>((true, null));
        }

        private void NotifyLocation()
        {
            if (cesiumGeoreference == null)
            {
                return;
            }

            var unityPosition = new double3(targetTransform.position);
            var ecefPosition = cesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed(unityPosition);
            var geodeticPosition = GeographicCoordinateConversion.EcefToGeodetic(ecefPosition.x, ecefPosition.y, ecefPosition.z);

            var now = DateTimeOffset.Now;
            var geographicLocation = new GeographicLocation(geodeticPosition.Latitude, geodeticPosition.Longitude, geodeticPosition.EllipsoidalHeight, now);

            // Cardinal direction
            var unityDirection = new double3(targetTransform.forward);
            var ecefDirection = cesiumGeoreference.TransformUnityDirectionToEarthCenteredEarthFixed(unityDirection);
            var enuDirection = GeographicCoordinateConversion.EcefToEnu(
                ecefPosition.x + ecefDirection.x, ecefPosition.y + ecefDirection.y, ecefPosition.z + ecefDirection.z,
                geodeticPosition.Latitude, geodeticPosition.Longitude, geodeticPosition.EllipsoidalHeight);
            var headingDegrees = (float)(math.atan2(enuDirection.East, enuDirection.North) * 180.0 / math.PI);

            var cardinalDirection = new CardinalDirection(headingDegrees, now);

            try
            {
                OnLocationUpdated?.Invoke(geographicLocation);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            try
            {
                OnDirectionUpdated?.Invoke(cardinalDirection);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
