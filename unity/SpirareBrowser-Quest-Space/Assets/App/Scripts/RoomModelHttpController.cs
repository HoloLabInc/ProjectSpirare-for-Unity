using HoloLab.UniWebServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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


    public class RoomModelHttpController : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private Material exportedSceneMaterial;

        private readonly OVRSceneGlbExporter sceneRoomGlbExporter = new OVRSceneGlbExporter();

        [Route("room")]
        public string RoomDownloadPage()
        {
            var globalMeshAnchors = OVRSceneFinder.FindGlobalMeshAnchors();
            var globalMeshHtml = @"
<div>
  <h2>Global Mesh</h2>
  <div>
";
            foreach (var globalMeshAnchor in globalMeshAnchors)
            {
                var id = globalMeshAnchor.Uuid.ToString();
                globalMeshHtml += $@"<p><a href=""/room/download/{id}"">{id}</a></p>";
            }
            globalMeshHtml += @"
  </div>
</div>
";

            var floorAnchors = OVRSceneFinder.FindFloorAnchors();
            var scenePlanesHtml = @"
<div>
  <h2>Scene Plane</h2>
  <div>
";
            foreach (var floorAnchor in floorAnchors)
            {
                var id = floorAnchor.Uuid.ToString();
                scenePlanesHtml += $@"<p><a href=""/room/download/{id}"">{id}</a></p>";
            }
            scenePlanesHtml += @"
  </div>
</div>
";

            var html =
                $@"
<html>
  <body>
    {globalMeshHtml}
    {scenePlanesHtml}
  </body>
</html>";

            return html;
        }

        [Route("room/download/:id")]
        public async Task<byte[]> FileDownload(HttpListenerResponse response, string id)
        {
            if (OVRSceneFinder.TryGetSceneByAnchorId(id, out var sceneRoom, out var sceneAnchor) == false)
            {
                response.StatusCode = 400;
                return Array.Empty<byte>();
            }

            (bool Success, byte[] Data) result;

            var classification = sceneAnchor.GetComponent<OVRSemanticClassification>();
            if (OVRSceneFinder.IsGlobalMesh(classification))
            {
                result = await sceneRoomGlbExporter.TryExportGlobalMeshAsync(sceneAnchor, exportedSceneMaterial);
            }
            else
            {
                result = await sceneRoomGlbExporter.TryExportScenePlanesAsync(sceneRoom, sceneAnchor, exportedSceneMaterial);
            }

            if (result.Success)
            {
                response.AppendHeader("Content-Disposition", $"attachment; filename=\"{id}.glb\"");
                return result.Data;
            }
            else
            {
                response.StatusCode = 500;
                return Array.Empty<byte>();
            }
        }
    }
}