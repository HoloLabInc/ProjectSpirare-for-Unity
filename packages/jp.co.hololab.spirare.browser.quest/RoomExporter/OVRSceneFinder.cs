using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpirareBrowser.Quest.Space
{
    public class OVRSceneFinder
    {
        public static OVRSceneAnchor[] FindGlobalMeshAnchors()
        {
            var sceneAnchors = new List<OVRSceneAnchor>();

            var sceneRooms = UnityEngine.Object.FindObjectsOfType<OVRSceneRoom>();
            foreach (var sceneRoom in sceneRooms)
            {
                if (TryGetGlobalMeshAnchor(sceneRoom, out var globalMeshObject))
                {
                    if (globalMeshObject.TryGetComponent<OVRSceneAnchor>(out var sceneAnchor))
                    {
                        sceneAnchors.Add(sceneAnchor);
                    }
                }
            }

            return sceneAnchors.ToArray();
        }

        public static OVRSceneAnchor[] FindFloorAnchors()
        {
            var sceneAnchors = new List<OVRSceneAnchor>();

            var sceneRooms = UnityEngine.Object.FindObjectsOfType<OVRSceneRoom>();
            foreach (var sceneRoom in sceneRooms)
            {
                var floor = sceneRoom.Floor;
                if (floor.TryGetComponent<OVRSceneAnchor>(out var sceneAnchor))
                {
                    sceneAnchors.Add(sceneAnchor);
                }
            }

            return sceneAnchors.ToArray();
        }

        public static bool TryGetSceneByAnchorId(string id, out OVRSceneRoom sceneRoom, out OVRSceneAnchor sceneAnchor)
        {
            var sceneRooms = UnityEngine.Object.FindObjectsOfType<OVRSceneRoom>();
            foreach (var room in sceneRooms)
            {
                var anchors = room.GetComponentsInChildren<OVRSceneAnchor>();

                foreach (var anchor in anchors)
                {
                    var anchorId = anchor.Uuid.ToString();
                    if (anchorId == id)
                    {
                        sceneRoom = room;
                        sceneAnchor = anchor;
                        return true;
                    }
                }
            }

            sceneRoom = null;
            sceneAnchor = null;
            return false;
        }

        public static bool IsGlobalMesh(OVRSemanticClassification ovrSemanticClassification)
        {
            if (ovrSemanticClassification == null)
            {
                return false;
            }

            return ovrSemanticClassification.Labels.Contains(OVRSceneManager.Classification.GlobalMesh);
        }

        private static bool TryGetGlobalMeshAnchor(OVRSceneRoom sceneRoom, out GameObject globalMeshObject)
        {
            var classifications = sceneRoom.GetComponentsInChildren<OVRSemanticClassification>();
            foreach (var classification in classifications)
            {
                if (IsGlobalMesh(classification))
                {
                    globalMeshObject = classification.gameObject;
                    return true;
                }
            }

            globalMeshObject = null;
            return false;
        }
    }
}