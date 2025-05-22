using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    public enum UnlitMaterialType
    {
        Unlit_Color = 0,
        UniversalRenderPipeline_Unlit
    }

    public static class UnlitMaterialUtility
    {
        public static Material CreateUnlitMaterial(UnlitMaterialType type, Color color, bool enableAlpha)
        {
            Material material = null;
            switch (type)
            {
                case UnlitMaterialType.Unlit_Color:
                    material = new Material(Shader.Find("Unlit/Color"));
                    if (material != null)
                    {
                        material.color = color;
                    }
                    break;
                case UnlitMaterialType.UniversalRenderPipeline_Unlit:
                    material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    if (material != null)
                    {
                        URPUnlitMaterialUtility.SetColor(material, color, enableAlpha);
                    }
                    break;
            }

            return material;
        }
    }

    public static class URPUnlitMaterialUtility
    {
        public static void SetColor(Material material, Color color, bool enableAlpha)
        {
            var transparent = enableAlpha && color.a < 1f;
            SetTransparent(material, transparent);

            material.SetColor("_BaseColor", color);
        }

        private static void SetTransparent(Material material, bool transparent)
        {
            if (transparent)
            {
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);

                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);

                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);

                material.SetFloat("_Surface", 0f);
                material.SetFloat("_Blend", 0f);

                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = (int)RenderQueue.Geometry;
            }
        }
    }
}

