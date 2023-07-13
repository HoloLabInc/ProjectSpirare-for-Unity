using HoloLab.PositioningTools.CoordinateSystem;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLab.Spirare
{
    [ExecuteAlways]
    public sealed class PomlObjectElementComponent : PomlElementComponent
    {
        private Transform cameraTransform;
        private WebSocketHelper webSocket;

        public event Action OnSelect;

        public bool Equipable
        {
            get
            {
                var element = PomlElement;
                if (element == null) { return false; }
                return (element.Attribute & ElementAttributeType.Equipable) == ElementAttributeType.Equipable;
            }
        }

        public override void Initialize(PomlElement element)
        {
            base.Initialize(element);

            GetCameraTransformPeriodically().Forget();

            UpdateGameObject(PomlElement);

            OnElementUpdated += UpdateGameObject;

            ConnectWebSocketAsync().Forget();
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // If controlling rotation with Billboard, disable rotation control by WorldCoordinateOrigin.
            if (TryGetComponent<WorldCoordinateOrigin>(out var worldCoordinateOrigin))
            {
                switch (PomlElement.RotationMode)
                {
                    case PomlRotationMode.Billboard:
                    case PomlRotationMode.VerticalBillboard:
                        worldCoordinateOrigin.BindRotation = false;
                        break;
                }
            }
        }

        private async UniTask ConnectWebSocketAsync()
        {
            var wsRecvUrl = PomlElement.WsRecvUrl;

            if (string.IsNullOrEmpty(wsRecvUrl) == false)
            {
                webSocket = new WebSocketHelper(this, PomlElement);

                var ct = this.GetCancellationTokenOnDestroy();
                await webSocket.Connect(wsRecvUrl, ct);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            // if (!Application.isPlaying)
            // {
            //    AddElementComponentToChildren();
            // }
#endif

            if (Application.isPlaying)
            {
                InvokeOnUpdate();
                UpdateRotation();
                UpdateScale();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.webSocket?.Dispose();
            this.webSocket = null;
            OnElementUpdated -= UpdateGameObject;
        }

        private void UpdateGameObject(PomlElement element)
        {
            transform.localPosition = CoordinateUtility.ToUnityCoordinate(element.Position, directional: true);
            transform.localRotation = CoordinateUtility.ToUnityCoordinate(element.Rotation);
            transform.localScale = CoordinateUtility.ToUnityCoordinate(element.Scale, directional: false);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add PomlElementComponent to a child object
        /// </summary>
        private void AddElementComponentToChildren()
        {
            /*
            if (gameObject.GetComponent<ModelElementComponent>() != null)
            {
                return;
            }
            */

            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<PomlObjectElementComponent>(out _))
                {
                    continue;
                }
                child.gameObject.AddComponent<PomlObjectElementComponent>();

                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefab == null)
                {
                    continue;
                }

                /*
                var assetPath = AssetDatabase.GetAssetPath(prefab);
                if (assetPath != null && assetPath.EndsWith(".glb"))
                {
                    child.gameObject.AddComponent<ModelElementComponent>();
                }
                */
            }
        }
#endif

        public void Select()
        {
            // Display the linked web page in a browser.
            var webLink = PomlElement.WebLink;
            if (!string.IsNullOrEmpty(webLink))
            {
                Application.OpenURL(webLink);
            }

            OnSelect?.Invoke();
        }

        private async UniTask GetCameraTransformPeriodically()
        {
            var token = this.GetCancellationTokenOnDestroy();
            while (Application.isPlaying)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                GetCameraTransform();
                await UniTask.Delay(5000, cancellationToken: token);
            }
        }

        private void GetCameraTransform()
        {
            if (cameraTransform != null)
            {
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        private void UpdateRotation()
        {
            if (cameraTransform == null)
            {
                return;
            }

            switch (PomlElement.RotationMode)
            {
                case PomlRotationMode.None:
                    return;
                case PomlRotationMode.Billboard:
                    {
                        transform.LookAt(cameraTransform);
                        break;
                    }
                case PomlRotationMode.VerticalBillboard:
                    {
                        var target = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
                        transform.LookAt(target);
                        break;
                    }
                default:
                    return;
            }
        }

        private void UpdateScale()
        {
            if (cameraTransform == null)
            {
                return;
            }

            if (PomlElement.ScaleByDistance.HasValue == false)
            {
                return;
            }

            var distance = Vector3.Distance(cameraTransform.position, transform.position);
            var localScale = distance * PomlElement.ScaleByDistance.Value * PomlElement.Scale;

            // Ensure that the scale does not fall below the minimum scale.
            if (PomlElement.MinScale.HasValue)
            {
                var minScale = PomlElement.MinScale.Value;
                localScale.x = Math.Max(localScale.x, minScale.x);
                localScale.y = Math.Max(localScale.y, minScale.y);
                localScale.z = Math.Max(localScale.z, minScale.z);
            }

            // Ensure that the scale does not exceed the maximum scale.
            if (PomlElement.MaxScale.HasValue)
            {
                var maxScale = PomlElement.MaxScale.Value;
                localScale.x = Math.Min(localScale.x, maxScale.x);
                localScale.y = Math.Min(localScale.y, maxScale.y);
                localScale.z = Math.Min(localScale.z, maxScale.z);
            }

            transform.localScale = localScale;
        }
    }
}
