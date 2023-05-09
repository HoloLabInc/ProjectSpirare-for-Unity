using UnityEngine;

namespace HoloLab.Spirare.Wasm.Core.Spirare
{
    internal static class CameraDescriptorHelper
    {
        public const int MainCameraDescriptor = 1;

        public static bool TryGetCamera(int cameraDescriptor, out Camera camera, out int errorCode)
        {
            switch (cameraDescriptor)
            {
                case MainCameraDescriptor:
                    camera = Camera.main;
                    errorCode = (int)Errno.Success;
                    return true;
                default:
                    camera = null;
                    errorCode = (int)Errno.CameraNotFount;
                    return false;
            }
        }
    }
}
