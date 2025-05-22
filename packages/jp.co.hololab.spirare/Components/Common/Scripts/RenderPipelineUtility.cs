using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    public enum RenderPipelineType
    {
        BuiltIn = 0,
        URP,
        HDRP,
    }

    public static class RenderPipelineUtility
    {
        public static RenderPipelineType GetRenderPipelineType()
        {
            var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;

            if (renderPipelineAsset == null)
            {
                return RenderPipelineType.BuiltIn;
            }

            var typeName = renderPipelineAsset.GetType().Name;
            if (typeName.Contains("UniversalRenderPipelineAsset"))
            {
                return RenderPipelineType.URP;
            }
            else if (typeName.Contains("HDRenderPipelineAsset"))
            {
                return RenderPipelineType.HDRP;
            }
            else
            {
                Debug.LogWarning($"Unknown render pipeline type: {typeName}");
                return RenderPipelineType.BuiltIn;
            }
        }
    }
}

