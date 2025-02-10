using HoloLab.Spirare.Pcx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    [RequireComponent(typeof(VisualEffect))]
    public class PointCloudVfxComponent : MonoBehaviour
    {
        [Serializable]
        public class VfxAssetWithCapacity
        {
            public int Capacity;
            public VisualEffectAsset VfxAsset;
        }

        [SerializeField]
        private List<VfxAssetWithCapacity> vfxAssets;

        private VisualEffect vfx;

        private void Awake()
        {
            vfx = GetComponent<VisualEffect>();
        }

        public void SetBakedPointCloud(BakedPointCloud bakedPointCloud)
        {
            var pointCount = bakedPointCloud.pointCount;
            SelectVisualEffectAsset(pointCount);

            vfx.SetUInt("PointCount", (uint)pointCount);
            var bounds = bakedPointCloud.bounds;
            vfx.SetVector3("BoundsCenter", bounds.center);
            vfx.SetVector3("BoundsSize", bounds.size);
            vfx.SetTexture("PositionMap", bakedPointCloud.positionMap);
            vfx.SetTexture("ColorMap", bakedPointCloud.colorMap);
        }

        private void SelectVisualEffectAsset(int pointCount)
        {
            foreach (var vfxAssetWithCapacity in vfxAssets)
            {
                if (pointCount <= vfxAssetWithCapacity.Capacity)
                {
                    vfx.visualEffectAsset = vfxAssetWithCapacity.VfxAsset;
                    return;
                }
            }

            var lastAsset = vfxAssets.Last();
            vfx.visualEffectAsset = lastAsset.VfxAsset;
            Debug.LogWarning($"The point count {pointCount} exceeds the capacity of the VFX asset ({lastAsset.Capacity}).");
        }

        public void SetPointSize(float pointSize)
        {
            if (pointSize == 0)
            {
                vfx.SetFloat("PointSize", 0.001f);
                vfx.SetBool("ScreenSpaceSizeEnabled", true);
            }
            else
            {
                vfx.SetFloat("PointSize", pointSize);
                vfx.SetBool("ScreenSpaceSizeEnabled", false);
            }
        }
    }
}

