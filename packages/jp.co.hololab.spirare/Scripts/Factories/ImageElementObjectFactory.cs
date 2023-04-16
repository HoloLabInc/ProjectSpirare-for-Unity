using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class ImageElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(PomlImageElement element, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
