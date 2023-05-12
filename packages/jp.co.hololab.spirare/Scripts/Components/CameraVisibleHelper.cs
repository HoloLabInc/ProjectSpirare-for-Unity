using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    [RequireComponent(typeof(Renderer))]
    public sealed class CameraVisibleHelper : MonoBehaviour
    {
        private Renderer _renderer;
        private Plane[] frustrumPlanes = new Plane[6];

        public bool IsInsideCameraBounds(Camera camera)
        {
            if (_renderer == null)
            {
                if (TryGetComponent<Renderer>(out _renderer) == false)
                {
                    return false;
                }
            }

            var renderer = _renderer;
            if (renderer.enabled == false) { return false; }
            if (gameObject.activeInHierarchy == false) { return false; }

            GeometryUtility.CalculateFrustumPlanes(camera, frustrumPlanes);
            var result = GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds);
            return result;
        }
    }
}
