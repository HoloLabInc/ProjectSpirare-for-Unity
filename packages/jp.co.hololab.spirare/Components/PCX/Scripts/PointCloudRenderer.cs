// This file includes code derived from Pcx - Point cloud importer & renderer for Unity
// (https://github.com/keijiro/Pcx) under the Unlicense License.
// The original code was developed by keijiro.

// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare.Pcx
{
    /// A renderer class that renders a point cloud contained by PointCloudData.
    public sealed class PointCloudRenderer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] PointCloudData _sourceData = null;

        public PointCloudData sourceData
        {
            get { return _sourceData; }
            set { _sourceData = value; }
        }

        [SerializeField] Color _pointTint = new Color(0.5f, 0.5f, 0.5f, 1);

        public Color pointTint
        {
            get { return _pointTint; }
            set { _pointTint = value; }
        }

        [SerializeField] float _pointSize = 0.05f;

        public float pointSize
        {
            get { return _pointSize; }
            set { _pointSize = value; }
        }

        [SerializeField] Material _pointMaterialBase = null;
        [SerializeField] Material _diskMaterialBase = null;

        #endregion

        #region Public properties (nonserialized)

        public ComputeBuffer sourceBuffer { get; set; }

        #endregion

        #region Internal resources

        [SerializeField, HideInInspector] Shader _pointShader = null;
        [SerializeField, HideInInspector] Shader _diskShader = null;

        #endregion

        #region Private objects

        Material _pointMaterial;
        Material _diskMaterial;
        Camera _currentCamera;

        bool _useScriptableRenderPipeline;

        #endregion

        #region MonoBehaviour implementation

        private void Awake()
        {
            _pointMaterial = new Material(_pointMaterialBase);
            _pointMaterial.hideFlags = HideFlags.DontSave;
            _pointMaterial.EnableKeyword("_COMPUTE_BUFFER");

            _diskMaterial = new Material(_diskMaterialBase);
            _diskMaterial.hideFlags = HideFlags.DontSave;
            _diskMaterial.EnableKeyword("_COMPUTE_BUFFER");

            _useScriptableRenderPipeline = GraphicsSettings.defaultRenderPipeline != null;
        }

        void OnValidate()
        {
            _pointSize = Mathf.Max(0, _pointSize);
        }

        void OnEnable()
        {
            if (_useScriptableRenderPipeline)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            }
        }

        void OnDisable()
        {
            if (_useScriptableRenderPipeline)
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            }
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _currentCamera = camera;
        }

        void OnDestroy()
        {
            if (_pointMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_pointMaterial);
                    Destroy(_diskMaterial);
                }
                else
                {
                    DestroyImmediate(_pointMaterial);
                    DestroyImmediate(_diskMaterial);
                }
            }
        }

        void OnRenderObject()
        {
            // We need a source data or an externally given buffer.
            if (_sourceData == null && sourceBuffer == null) return;

            // Check the camera condition.
            if (_useScriptableRenderPipeline == false)
            {
                _currentCamera = Camera.current;
            }

            if (_currentCamera == null) return;
            if ((_currentCamera.cullingMask & (1 << gameObject.layer)) == 0) return;
            if (_currentCamera.name == "Preview Scene Camera") return;

            // TODO: Do view frustum culling here.

            // Lazy initialization
            if (_pointMaterial == null)
            {
                _pointMaterial = new Material(_pointShader);
                _pointMaterial.hideFlags = HideFlags.DontSave;
                _pointMaterial.EnableKeyword("_COMPUTE_BUFFER");

                _diskMaterial = new Material(_diskShader);
                _diskMaterial.hideFlags = HideFlags.DontSave;
                _diskMaterial.EnableKeyword("_COMPUTE_BUFFER");
            }

            // Use the external buffer if given any.
            var pointBuffer = sourceBuffer != null ?
                sourceBuffer : _sourceData.computeBuffer;

            if (_pointSize == 0)
            {
                _pointMaterial.SetPass(0);
                _pointMaterial.SetColor("_Tint", _pointTint);
                _pointMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _pointMaterial.SetBuffer("_PointBuffer", pointBuffer);
#if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
#else
                Graphics.DrawProcedural(MeshTopology.Points, pointBuffer.count, 1);
#endif
            }
            else
            {
                _diskMaterial.SetPass(0);
                _diskMaterial.SetColor("_Tint", _pointTint);
                _diskMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _diskMaterial.SetBuffer("_PointBuffer", pointBuffer);
                _diskMaterial.SetFloat("_PointSize", pointSize);
#if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
#else
                Graphics.DrawProcedural(MeshTopology.Points, pointBuffer.count, 1);
#endif
            }
        }

        #endregion
    }
}
