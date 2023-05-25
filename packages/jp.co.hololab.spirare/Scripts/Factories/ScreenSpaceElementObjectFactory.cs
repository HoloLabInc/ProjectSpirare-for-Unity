using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class ScreenSpaceElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(PomlScreenSpaceElement element, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
