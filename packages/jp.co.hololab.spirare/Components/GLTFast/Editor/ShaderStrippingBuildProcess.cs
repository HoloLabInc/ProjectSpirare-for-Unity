#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    class ShaderStrippingBuildProcessor : IPreprocessShaders
    {
        public int callbackOrder => 0;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData)
        {
            Debug.Log("----");
            Debug.Log(shader.name);
            Debug.Log(snippet.passName);
            Debug.Log(snippet.passType);

            var targetShaderName = new string[]
            {
                "Shader Graphs/glTF-pbrSpecularGlossiness",
                "Shader Graphs/glTF-pbrMetallicRoughness"
            };

            var strippedPassName = new string[0];

            var renderPipeline = GraphicsSettings.currentRenderPipeline;
            if (renderPipeline != null)
            {
                // for URP/HDRP
                strippedPassName = new string[]
                {
                    "BuiltIn Forward",
                    "BuiltIn ForwardAdd",
                    "BuiltIn Deferred",
                };
            }

            if (targetShaderName.Contains(shader.name) && strippedPassName.Contains(snippet.passName))
            {
                Debug.Log("trim!!");
                shaderCompilerData.Clear();
                return;
            }
        }
    }
}
#endif
