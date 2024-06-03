#if ARCOREEXTENSIONS_1_37_0_OR_NEWER
using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
#endif
using UnityEngine;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryController : MonoBehaviour
    {
        [Flags]
        public enum StreetscapeGeometryOcclusionType
        {
            None = 0,
            Building = 1 << 0,
            Terrain = 1 << 1,
            All = Building | Terrain
        }

#if ARCOREEXTENSIONS_1_37_0_OR_NEWER
        [SerializeField]
        private Material streetscapeGeometryMaterial;

        private ARStreetscapeGeometryManager arStreetscapeGeometryManager;

        private StreetscapeGeometryOcclusionType occlusionType = StreetscapeGeometryOcclusionType.None;

        private readonly Dictionary<TrackableId, (GameObject GameObject, StreetscapeGeometryType GeometryType)> streetscapeGeometryGameObjects =
            new Dictionary<TrackableId, (GameObject, StreetscapeGeometryType)>();

        private void Awake()
        {
            arStreetscapeGeometryManager = FindObjectOfType<ARStreetscapeGeometryManager>();
            if (arStreetscapeGeometryManager == null)
            {
                Debug.LogWarning($"{nameof(ARStreetscapeGeometryManager)} not found in scene");
            }
        }

        private void OnEnable()
        {
            if (arStreetscapeGeometryManager != null)
            {
                arStreetscapeGeometryManager.StreetscapeGeometriesChanged += StreetscapeGeometriesChanged;
            }
        }

        private void OnDisable()
        {
            if (arStreetscapeGeometryManager != null)
            {
                arStreetscapeGeometryManager.StreetscapeGeometriesChanged -= StreetscapeGeometriesChanged;
            }

            DestroyAllGeometryObjects();
        }

        public void SetOcclusionType(StreetscapeGeometryOcclusionType occlusionType)
        {
            this.occlusionType = occlusionType;

            if (occlusionType == StreetscapeGeometryOcclusionType.None)
            {
                arStreetscapeGeometryManager.gameObject.SetActive(false);
                DestroyAllGeometryObjects();
            }
            else
            {
                arStreetscapeGeometryManager.gameObject.SetActive(true);

                foreach (var geometryObject in streetscapeGeometryGameObjects.Values)
                {
                    var active = ShouldBeActive(geometryObject.GeometryType, occlusionType);
                    geometryObject.GameObject.SetActive(active);
                }
            }
        }

        private void StreetscapeGeometriesChanged(ARStreetscapeGeometriesChangedEventArgs eventArgs)
        {
            AddGeometries(eventArgs.Added);
            UpdateGeometries(eventArgs.Updated);
            RemoveGeometries(eventArgs.Removed);
        }

        private void AddGeometries(List<ARStreetscapeGeometry> added)
        {
            foreach (var streetscapeGeometry in added)
            {
                CreateGeometryObject(streetscapeGeometry);
            }
        }

        private void UpdateGeometries(List<ARStreetscapeGeometry> updated)
        {
            foreach (var streetscapeGeometry in updated)
            {
                CreateGeometryObject(streetscapeGeometry);
                UpdateGeometryObject(streetscapeGeometry);
            }
        }

        private void RemoveGeometries(List<ARStreetscapeGeometry> removed)
        {
            foreach (var streetscapeGeometry in removed)
            {
                DestroyGeometryObject(streetscapeGeometry);
            }
        }

        private void CreateGeometryObject(ARStreetscapeGeometry streetscapeGeometry)
        {
            if (streetscapeGeometry.mesh == null)
            {
                return;
            }

            var trackableId = streetscapeGeometry.trackableId;

            if (streetscapeGeometryGameObjects.ContainsKey(trackableId))
            {
                return;
            }

            GameObject renderObject = new GameObject(
                           "StreetscapeGeometryMesh", typeof(MeshFilter), typeof(MeshRenderer));

            if (renderObject)
            {
                renderObject.transform.parent = transform;
                renderObject.transform.SetPositionAndRotation(streetscapeGeometry.pose.position, streetscapeGeometry.pose.rotation);
                renderObject.GetComponent<MeshFilter>().mesh = streetscapeGeometry.mesh;
                renderObject.GetComponent<MeshRenderer>().material = streetscapeGeometryMaterial;

                // streetscapeGeometry.streetscapeGeometryType == StreetscapeGeometryType

                var geometryType = streetscapeGeometry.streetscapeGeometryType;
                /*
                var geometryType = streetscapeGeometry.streetscapeGeometryType;

                if (streetscapeGeometry.streetscapeGeometryType == StreetscapeGeometryType.Terrain)
                {
                    renderObject.SetActive(false);
                }
                */
                var active = ShouldBeActive(geometryType, occlusionType);
                renderObject.SetActive(active);

                streetscapeGeometryGameObjects[trackableId] = (renderObject, geometryType);
            }
        }

        private void UpdateGeometryObject(ARStreetscapeGeometry streetscapeGeometry)
        {
            if (streetscapeGeometryGameObjects.TryGetValue(streetscapeGeometry.trackableId, out var geometryObject))
            {
                if (geometryObject.GameObject != null)
                {
                    geometryObject.GameObject.transform.SetPositionAndRotation(streetscapeGeometry.pose.position, streetscapeGeometry.pose.rotation);
                }
            }
        }

        private void DestroyGeometryObject(ARStreetscapeGeometry streetscapeGeometry)
        {
            if (streetscapeGeometryGameObjects.TryGetValue(streetscapeGeometry.trackableId, out var geometryObject))
            {
                streetscapeGeometryGameObjects.Remove(streetscapeGeometry.trackableId);
                Destroy(geometryObject.GameObject);
            }
        }

        private void DestroyAllGeometryObjects()
        {
            foreach (var pair in streetscapeGeometryGameObjects)
            {
                Destroy(pair.Value.GameObject);
            }

            streetscapeGeometryGameObjects.Clear();
        }

        private static bool ShouldBeActive(StreetscapeGeometryType streetscapeGeometryType, StreetscapeGeometryOcclusionType occlusionType)
        {
            return streetscapeGeometryType switch
            {
                StreetscapeGeometryType.Terrain => occlusionType.HasFlag(StreetscapeGeometryOcclusionType.Terrain),
                StreetscapeGeometryType.Building => occlusionType.HasFlag(StreetscapeGeometryOcclusionType.Building),
                _ => false,
            };
        }
#endif
    }
}

