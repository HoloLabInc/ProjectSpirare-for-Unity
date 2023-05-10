using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class CameraVisibleHelper : MonoBehaviour
    {
        private readonly List<Camera> _isInsideCameraBounds = new List<Camera>();

        public bool IsInsideCameraBounds(Camera camera)
        {
            return _isInsideCameraBounds.Contains(camera);
        }

        private void LateUpdate()
        {
            _isInsideCameraBounds.Clear();
        }

        private void OnWillRenderObject()
        {
            var currentCamera = Camera.current;

#if UNITY_EDITOR
            if (currentCamera.name != "SceneCamera" && currentCamera.name != "Preview Camera")
#endif
            {
                _isInsideCameraBounds.Add(currentCamera);
            }
        }
    }
}
