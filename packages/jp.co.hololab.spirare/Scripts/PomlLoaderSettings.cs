using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HoloLab.Spirare
{
    public class PomlLoaderSettings : ScriptableObject
    {
        public ModelElementObjectFactory modelElementObjectFactory;
        public ImageElementObjectFactory imageElementObjectFactory;
        public VideoElementObjectFactory videoElementObjectFactory;
        public TextElementObjectFactory textElementObjectFactory;
        public GeometryElementObjectFactory geometryElementObjectFactory;
        public ScreenSpaceElementObjectFactory screenSpaceElementObjectFactory;

        [FormerlySerializedAs("placementElementComponentFactory")]
        public SpaceReferenceElementComponentFactory spaceReferenceElementComponentFactory;

        [FormerlySerializedAs("geoPlacementElementComponentFactory")]
        public GeoReferenceElementComponentFactory geoReferenceElementComponentFactory;

        public ScriptElementComponentFactory scriptElementComponentFactory;
        public Cesium3dTilesElementFactory cesium3dTilesElementFactory;

        public Material occlusionMaterial;

        public string defaultLayerName;
        public string screenSpaceLayerName;

        public int DefaultLayer => ConvertLayerNameToLayer(defaultLayerName);
        public int ScreenSpaceLayer => ConvertLayerNameToLayer(screenSpaceLayerName);

        private static int ConvertLayerNameToLayer(string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                return 0;
            }
            else
            {
                return layer;
            }
        }
    }
}
