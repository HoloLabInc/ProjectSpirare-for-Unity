using HoloLab.UniWebServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace SpirareBrowser.Quest.Space
{
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
    <h1>Download Page for Room Models</h1>
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