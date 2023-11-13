using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast.Logging;
using System.Threading.Tasks;
using GLTFast.Export;
using System.IO;

namespace SpirareBrowser.Quest.Space
{
    public class OVRSceneGlbExporter
    {
        public async Task<(bool Success, byte[] Data)> TryExportScenePlanesAsync(OVRSceneRoom sceneRoom, OVRSceneAnchor originAnchor, Material material)
        {
            var exportObject = CreateScenePlanesGameObjectForExport(sceneRoom, originAnchor, material);
            var result = await ExportAsGlbAsync(exportObject);
            return result;
        }

        public async Task<(bool Success, byte[] Data)> TryExportGlobalMeshAsync(OVRSceneAnchor globalMeshAnchor, Material material)
        {
            var exportObject = CreateGlobalMeshGameObjectForExport(globalMeshAnchor, material);
            var result = await ExportAsGlbAsync(exportObject);
            return result;
        }

        private static GameObject CreateScenePlanesGameObjectForExport(OVRSceneRoom sceneRoom, OVRSceneAnchor originAnchor, Material material)
        {
            var exportOrigin = new GameObject();
            exportOrigin.SetActive(false);
            exportOrigin.transform.SetPositionAndRotation(originAnchor.transform.position, originAnchor.transform.rotation);

            var exportPlanes = new List<OVRScenePlane>
            {
                sceneRoom.Floor,
                sceneRoom.Ceiling
            };
            exportPlanes.AddRange(sceneRoom.Walls);

            foreach (var plane in exportPlanes)
            {
                CreateScenePlaneGameObject(plane, exportOrigin.transform, material);
            }

            var originRotation = Quaternion.Euler(-90, 180, 0);
            exportOrigin.transform.SetPositionAndRotation(Vector3.zero, originRotation);

            return exportOrigin;
        }

        private static void CreateScenePlaneGameObject(OVRScenePlane scenePlane, Transform parent, Material material)
        {
            var meshFilter = scenePlane.GetComponent<MeshFilter>();

            var planeObject = new GameObject();

            planeObject.transform.SetPositionAndRotation(scenePlane.transform.position, scenePlane.transform.rotation);
            planeObject.transform.SetParent(parent, worldPositionStays: true);

            var planeObjectMeshFilter = planeObject.AddComponent<MeshFilter>();
            planeObjectMeshFilter.sharedMesh = meshFilter.sharedMesh;

            var planeObjectMeshRenderer = planeObject.AddComponent<MeshRenderer>();
            planeObjectMeshRenderer.sharedMaterial = material;
        }

        private static GameObject CreateGlobalMeshGameObjectForExport(OVRSceneAnchor globalMeshAnchor, Material material)
        {
            var exportOrigin = new GameObject();
            exportOrigin.SetActive(false);

            var globalMesh = globalMeshAnchor.GetComponent<MeshFilter>().sharedMesh;

            var exportMesh = new Mesh();
            exportMesh.indexFormat = globalMesh.indexFormat;
            exportMesh.vertices = globalMesh.vertices;
            exportMesh.uv = globalMesh.uv;
            exportMesh.triangles = globalMesh.triangles;
            exportMesh.RecalculateNormals();

            var meshFilter = exportOrigin.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = exportMesh;

            var meshRenderer = exportOrigin.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            var originRotation = Quaternion.Euler(-90, 180, 0);
            exportOrigin.transform.SetPositionAndRotation(Vector3.zero, originRotation);

            return exportOrigin;
        }

        private static async Task<(bool Result, byte[] Data)> ExportAsGlbAsync(GameObject exportObject)
        {
            var logger = new CollectingLogger();

            var exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,
            };

            var gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = false,
            };

            var export = new GameObjectExport(exportSettings, gameObjectExportSettings, logger: logger);
            export.AddScene(new GameObject[] { exportObject }, "Root");

            var memoryStream = new MemoryStream();
            var success = await export.SaveToStreamAndDispose(memoryStream);

            if (success)
            {
                return (true, memoryStream.ToArray());
            }
            else
            {
                Debug.LogError("Something went wrong exporting a glTF");
                // Log all exporter messages
                logger.LogAll();

                return (false, null);
            }
        }
    }
}
