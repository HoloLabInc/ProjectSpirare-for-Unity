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
#if ARCOREEXTENSIONS_1_37_0_OR_NEWER
        private ARStreetscapeGeometryManager arStreetscapeGeometryManager;

        [SerializeField]
        private Material streetscapeGeometryMaterial;

        private readonly Dictionary<TrackableId, GameObject> streetscapeGeometryGameObjects =
            new Dictionary<TrackableId, GameObject>();

        private void Awake()
        {
            arStreetscapeGeometryManager = FindObjectOfType<ARStreetscapeGeometryManager>();
            if (arStreetscapeGeometryManager != null)
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

                streetscapeGeometryGameObjects[trackableId] = renderObject;
            }
        }

        private void UpdateGeometryObject(ARStreetscapeGeometry streetscapeGeometry)
        {
            if (streetscapeGeometryGameObjects.TryGetValue(streetscapeGeometry.trackableId, out var renderObject))
            {
                renderObject.transform.SetPositionAndRotation(streetscapeGeometry.pose.position, streetscapeGeometry.pose.rotation);
            }
        }

        private void DestroyGeometryObject(ARStreetscapeGeometry streetscapeGeometry)
        {
            if (streetscapeGeometryGameObjects.TryGetValue(streetscapeGeometry.trackableId, out var renderObject))
            {
                streetscapeGeometryGameObjects.Remove(streetscapeGeometry.trackableId);
                Destroy(renderObject);
            }
        }

        private void DestroyAllGeometryObjects()
        {
            foreach (var pair in streetscapeGeometryGameObjects)
            {
                Destroy(pair.Value);
            }

            streetscapeGeometryGameObjects.Clear();
        }
#endif
    }
}
