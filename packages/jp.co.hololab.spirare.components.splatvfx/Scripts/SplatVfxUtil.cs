using Cysharp.Threading.Tasks;
using SplatVfx;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal static class SplatVfxUtil
    {
        public static GameObject InstantiateSplatVfx(VisualEffect splatPrefab, SplatData splatData, Transform parent)
        {
            var visualEffect = UnityEngine.Object.Instantiate(splatPrefab);
            var splatObject = visualEffect.gameObject;

            var (boundsCenter, boundsSize) = GetBounds(splatData);
            visualEffect.SetVector3("BoundsCenter", boundsCenter);
            visualEffect.SetVector3("BoundsSize", boundsSize);

            var binderBase = splatObject.AddComponent<VFXPropertyBinder>();
            var binder = binderBase.AddPropertyBinder<VFXSplatDataBinder>();
            binder.SplatData = splatData;

            splatObject.transform.SetParent(parent, worldPositionStays: false);

            return splatObject;
        }

        private static (Vector3 Center, Vector3 Size) GetBounds(SplatData splatData)
        {
            if (splatData.SplatCount == 0)
            {
                return (Vector3.zero, Vector3.zero);
            }

            var positions = splatData.PositionArray;

            var minX = positions.Min(p => p.x);
            var minY = positions.Min(p => p.y);
            var minZ = positions.Min(p => p.z);

            var maxX = positions.Max(p => p.x);
            var maxY = positions.Max(p => p.y);
            var maxZ = positions.Max(p => p.z);

            var center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            var size = new Vector3((maxX - minX) / 2, (maxY - maxY) / 2, (maxZ - minZ) / 2);

            return (center, size);
        }
    }
}
