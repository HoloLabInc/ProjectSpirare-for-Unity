#if UNITY_EDITOR && SPIRARE_DEV

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    public class VfxAssetCapacityVariationGenerator : EditorWindow
    {
        private VisualEffectAsset sourceVFX;

        private List<(string NameSuffix, int Capacity)> capacities = new List<(string, int)>()
        {
            ("64k", 0x10000),
            ("128k", 0x20000),
            ("256k", 0x40000),
            ("512k", 0x80000),
            ("1M", 0x100000),
            ("2M", 0x200000),
            ("4M", 0x400000),
            ("8M", 0x800000),
            ("16M", 0x1000000),
            ("32M", 0x2000000),
        };

        [MenuItem("Window/Spirare/VFX Capacity Variation Generator")]
        public static void ShowWindow()
        {
            GetWindow<VfxAssetCapacityVariationGenerator>("VFX Capacity Variation Generator");
        }

        private void OnGUI()
        {
            sourceVFX = EditorGUILayout.ObjectField("Source VFX Asset", sourceVFX, typeof(VisualEffectAsset), false) as VisualEffectAsset;

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate VFX Assets"))
            {
                GenerateVFXAssetsWithDifferentCapacities();
            }
        }

        private void GenerateVFXAssetsWithDifferentCapacities()
        {
            if (sourceVFX == null)
            {
                Debug.LogError("Source VFX Asset is not specified.");
                return;
            }

            var sourcePath = AssetDatabase.GetAssetPath(sourceVFX);
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("Failed to get the path of the source VFX Asset.");
                return;
            }

            var directory = Path.GetDirectoryName(sourcePath);
            var baseFileName = Path.GetFileNameWithoutExtension(sourcePath);

            foreach (var pair in capacities)
            {
                var newFileName = $"{baseFileName}_{pair.NameSuffix}.vfx";
                var newPath = Path.Combine(directory, newFileName);

                var vfxText = File.ReadAllText(sourcePath);

                var capacityRegex = new Regex(@"capacity:\s*\d+");
                var replacedText = capacityRegex.Replace(vfxText, $"capacity: {pair.Capacity}");

                File.WriteAllText(newPath, replacedText);

                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(newPath);
            }
        }
    }
}
#endif

