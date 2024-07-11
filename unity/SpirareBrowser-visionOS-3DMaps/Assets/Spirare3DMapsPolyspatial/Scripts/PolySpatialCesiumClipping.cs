using Unity.PolySpatial;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps.PolySpatial
{
    public class PolySpatialCesiumClipping : MonoBehaviour
    {
        [SerializeField]
        private Transform clippingOriginTransform;

        private void LateUpdate()
        {
            var worldToLocal = clippingOriginTransform.worldToLocalMatrix;
            PolySpatialShaderGlobals.SetMatrix("_ClippingOriginWorldToLocalMatrix", worldToLocal);
        }
    }
}
