using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class GeometryElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(
            PomlGeometryElement geometryElement,
            GeoReferenceElementComponentFactory geoReferenceElementComponentFactory,
            PomlLoadOptions loadOptions,
            Transform parentTransform = null);
    }
}
