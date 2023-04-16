using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class TextElementObjectFactory : ScriptableObject
    {
        public abstract GameObject CreateObject(PomlTextElement element, PomlLoadOptions loadOptions, Transform parentTransform = null);
    }
}
