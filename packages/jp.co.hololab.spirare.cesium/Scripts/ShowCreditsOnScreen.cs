using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class ShowCreditsOnScreen : MonoBehaviour
    {
        private Cesium3DTileset tileset;

        private void Start()
        {
            tileset = GetComponent<Cesium3DTileset>();
        }

        private void Update()
        {
            tileset.showCreditsOnScreen = true;
        }
    }
}
