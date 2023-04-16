using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class Cesium3dTilesElementFactory : ScriptableObject
    {
        public abstract GameObject Create(PomlCesium3dTilesElement cesium3dTilesElement, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
