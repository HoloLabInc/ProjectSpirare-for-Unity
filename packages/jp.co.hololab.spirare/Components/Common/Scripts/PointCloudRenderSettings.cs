using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    [CreateAssetMenu(fileName = "PointCloudRenderSettings", menuName = "Spirare/PointCloudRenderSettings", order = 1)]
    public class PointCloudRenderSettings : ScriptableObject
    {
        public float pointSize = 0f;
    }
}
