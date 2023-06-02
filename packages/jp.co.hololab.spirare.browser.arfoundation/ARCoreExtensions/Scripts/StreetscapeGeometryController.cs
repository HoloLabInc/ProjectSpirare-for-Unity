using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryController : MonoBehaviour
    {
#if ARCOREEXTENSIONS_1_37_0_OR_NEWER
        [SerializeField]
        private ARStreetscapeGeometryManager arStreetscapeGeometryManager;

        [SerializeField]
        private Material streetscapeGeometryMaterial;

        private readonly ConcurrentQueue<ARStreetscapeGeometriesChangedEventArgs> streetscapeGeometriesChangedEventQueue
            = new ConcurrentQueue<ARStreetscapeGeometriesChangedEventArgs>();

        private readonly Dictionary<TrackableId, GameObject> streetscapeGeometryGameObjects =
            new Dictionary<TrackableId, GameObject>();

        private void OnEnable()
        {
            arStreetscapeGeometryManager.StreetscapeGeometriesChanged += GetStreetscapeGeometry;
        }

        private void OnDisable()
        {
            arStreetscapeGeometryManager.StreetscapeGeometriesChanged -= GetStreetscapeGeometry;

            // Clear event queue
            while (streetscapeGeometriesChangedEventQueue.Count > 0)
            {
                streetscapeGeometriesChangedEventQueue.TryDequeue(out _);
            }

            DestroyAllGeometryObjects();
        }

        private void Update()
        {
            if (streetscapeGeometriesChangedEventQueue.TryDequeue(out var changedEvent))
            {
                ApplyChangedEvent(changedEvent);
            }
        }

        private void GetStreetscapeGeometry(ARStreetscapeGeometriesChangedEventArgs eventArgs)
        {
            streetscapeGeometriesChangedEventQueue.Enqueue(eventArgs);
        }

        private void ApplyChangedEvent(ARStreetscapeGeometriesChangedEventArgs eventArgs)
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
