using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public class ObjectManipulator : InteractableBase
    {
        [SerializeField]
        private bool rotationEnabledX;

        [SerializeField]
        private bool rotationEnabledY;

        [SerializeField]
        private bool rotationEnabledZ;

        [SerializeField]
        private bool translationEnabled;

        [SerializeField]
        private Transform targetTransform;

        public Transform TargetTransform
        {
            get
            {
                if (targetTransform == null)
                {
                    return transform;
                }
                return targetTransform;
            }
            set
            {
                targetTransform = value;
            }
        }

        private Pose initialPose;

        private Vector3 manipulationOffsetInWorldSpace;

        private Quaternion initialInputDeviceRotation;

        private int? interactionId;

        private bool IsRotationEnabled => rotationEnabledX || rotationEnabledY || rotationEnabledZ;

        private bool IsTranslationEnabled => translationEnabled;

        public event Action OnManipulationStarted;
        public event Action OnManipulationEnded;

        private void Awake()
        {
            if (TargetTransform == null)
            {
                TargetTransform = transform;
            }
        }

        public override void OnTouchBegan(SpatialPointerState touchData)
        {
            if (IsValidTouchKind(touchData) == false)
            {
                return;
            }

            if (interactionId.HasValue)
            {
                return;
            }

            interactionId = touchData.interactionId;
            initialPose = new Pose(TargetTransform.position, TargetTransform.rotation);
            manipulationOffsetInWorldSpace = touchData.interactionPosition - TargetTransform.position;
            initialInputDeviceRotation = touchData.inputDeviceRotation;

            try
            {
                OnManipulationStarted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnTouchMoved(SpatialPointerState touchData)
        {
            if (IsValidTouchKind(touchData) == false)
            {
                return;
            }

            if (touchData.interactionId != interactionId)
            {
                return;
            }

            if (IsTranslationEnabled && IsRotationEnabled)
            {
                var position = GetPositionAfterManipulation(touchData);
                var rotation = GetRotationAfterManipulation(touchData);
                TargetTransform.SetPositionAndRotation(position, rotation);
            }
            else if (IsTranslationEnabled)
            {
                var position = GetPositionAfterManipulation(touchData);
                TargetTransform.position = position;
            }
            else if (IsRotationEnabled)
            {
                var rotation = GetRotationAfterManipulation(touchData);
                TargetTransform.rotation = rotation;
            }
        }

        public override void OnTouchEnded(SpatialPointerState touchData)
        {
            if (touchData.interactionId == interactionId)
            {
                interactionId = null;

                try
                {
                    OnManipulationEnded?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private bool IsValidTouchKind(SpatialPointerState touchData)
        {
            switch (touchData.Kind)
            {
                case SpatialPointerKind.IndirectPinch:
                case SpatialPointerKind.DirectPinch:
                    return true;
                default:
                    return false;
            }
        }

        private Vector3 GetPositionAfterManipulation(SpatialPointerState touchData)
        {
            return touchData.interactionPosition - manipulationOffsetInWorldSpace;
        }

        private Quaternion GetRotationAfterManipulation(SpatialPointerState touchData)
        {
            var initialRotation = initialPose.rotation;
            var unconstraintedRotation = touchData.inputDeviceRotation * Quaternion.Inverse(initialInputDeviceRotation) * initialRotation;

            var rotationDelta = unconstraintedRotation * Quaternion.Inverse(initialRotation);
            var rotationDeltaEuler = rotationDelta.eulerAngles;

            if (rotationEnabledX == false)
            {
                rotationDeltaEuler.x = 0;
            }
            if (rotationEnabledY == false)
            {
                rotationDeltaEuler.y = 0;
            }
            if (rotationEnabledZ == false)
            {
                rotationDeltaEuler.z = 0;
            }

            var rotation = Quaternion.Euler(rotationDeltaEuler) * initialRotation;
            return rotation;
        }
    }
}
