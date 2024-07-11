using UnityEngine;
using UnityEngine.Video;

namespace HoloLab.Spirare.PolySpatial
{
    public class SetVideoElementRenderTextureDirty : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer videoPlayer;

        private void Update()
        {
            var renderTexture = videoPlayer.targetTexture;
            if (renderTexture != null)
            {
                Unity.PolySpatial.PolySpatialObjectUtils.MarkDirty(renderTexture);
            }
        }
    }
}
