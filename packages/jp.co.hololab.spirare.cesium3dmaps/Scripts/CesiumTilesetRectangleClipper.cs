using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Cesium3DTileset))]
public class CesiumTilesetRectangleClipper : MonoBehaviour
{
    private Cesium3DTileset tileset;

    private List<(GameObject TileObject, Material[] Materials)> tiles
        = new List<(GameObject TileObject, Material[] Materials)>();

    public Transform ClippingOriginTransform { get; set; }

    public float ClippingSizeX { get; set; } = 1;

    public float ClippingSizeZ { get; set; } = 1;

    private void Start()
    {
        tileset = GetComponent<Cesium3DTileset>();
        tileset.OnTileGameObjectCreated += Tileset_OnTileGameObjectCreated;
    }

    private void Tileset_OnTileGameObjectCreated(GameObject tileObject)
    {
        var meshRenderers = tileObject.GetComponentsInChildren<MeshRenderer>();
        var materials = meshRenderers.Select(x => x.material).ToArray();
        tiles.Add((tileObject, materials));
    }

    private void LateUpdate()
    {
        if (ClippingOriginTransform == null)
        {
            return;
        }

        var worldToLocal = ClippingOriginTransform.worldToLocalMatrix;

        tiles.RemoveAll(x => x.TileObject == null);

        foreach (var tile in tiles)
        {
            if (tile.TileObject.activeInHierarchy == false)
            {
                continue;
            }

            foreach (var tileMaterial in tile.Materials)
            {
                tileMaterial.SetMatrix("_ClippingOriginWorldToLocalMatrix", worldToLocal);
                tileMaterial.SetFloat("_ClippingSizeX", ClippingSizeX / 2);
                tileMaterial.SetFloat("_ClippingSizeZ", ClippingSizeZ / 2);
            }
        }
    }
}
