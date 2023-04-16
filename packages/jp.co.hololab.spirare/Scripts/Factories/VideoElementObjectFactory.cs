using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class VideoElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(PomlVideoElement element, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
