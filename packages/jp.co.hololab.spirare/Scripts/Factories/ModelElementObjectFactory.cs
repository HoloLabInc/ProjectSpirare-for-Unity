using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class ModelElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
