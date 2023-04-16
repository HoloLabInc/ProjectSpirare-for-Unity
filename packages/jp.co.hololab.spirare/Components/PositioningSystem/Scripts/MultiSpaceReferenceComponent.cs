using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class MultiSpaceReferenceComponent : MonoBehaviour
    {
        private CoordinateManager coordinateManager;

        private PomlSpaceReferenceElement bindingSpaceReferenceElement = null;
        private Transform bindingTransform;
        private Matrix4x4 spaceReferenceToObjectOrigin = Matrix4x4.identity;

        private List<PomlSpaceReferenceElement> spaceReferenceElements = new List<PomlSpaceReferenceElement>();

        private bool isBindingActive => bindingSpaceReferenceElement != null;

        public void Initialize()
        {
            gameObject.SetActive(false);

            coordinateManager = CoordinateManager.Instance;
            coordinateManager.OnSpaceBound += OnSpaceBound;
            coordinateManager.OnSpaceLost += OnSpaceLost;
        }

        private void Update()
        {
            if (bindingTransform != null)
            {
                UpdatePosition(bindingTransform.position, bindingTransform.rotation);
            }
        }

        private void OnDestroy()
        {
            coordinateManager.OnSpaceBound -= OnSpaceBound;
        }

        public void AddSpaceReference(PomlSpaceReferenceElement spaceReferenceElement)
        {
            spaceReferenceElements.Add(spaceReferenceElement);

            if (isBindingActive == false)
            {
                BindSpaceReference(spaceReferenceElement);
            }
        }

        private void OnSpaceBound(SpaceBinding spaceBinding)
        {
            if (isBindingActive)
            {
                return;
            }

            foreach (var spaceReferenceElement in spaceReferenceElements)
            {
                var success = BindSpace(spaceReferenceElement, spaceBinding);
                if (success)
                {
                    return;
                }
            }
        }

        private void OnSpaceLost(SpaceBinding spaceBinding)
        {
            if (isBindingActive == false)
            {
                return;
            }

            if (BindingIsValid(bindingSpaceReferenceElement, spaceBinding))
            {
                bindingTransform = null;
                bindingSpaceReferenceElement = null;
                spaceReferenceToObjectOrigin = Matrix4x4.identity;

                // Search other valid binding
                foreach (var spaceReferenceElement in spaceReferenceElements)
                {
                    var success = BindSpaceReference(spaceReferenceElement);
                    if (success)
                    {
                        return;
                    }
                }

                gameObject.SetActive(false);
            }
        }

        private void UpdatePosition(Vector3 bindingPosition, Quaternion bindingRotation)
        {
            var worldToSpaceReference = Matrix4x4.TRS(bindingPosition, bindingRotation, Vector3.one);
            var worldToObjectOrigin = worldToSpaceReference * spaceReferenceToObjectOrigin;

            var position = worldToObjectOrigin.ExtractPosition();
            var rotation = worldToObjectOrigin.ExtractRotation();
            transform.SetPositionAndRotation(position, rotation);
        }

        private bool BindSpaceReference(PomlSpaceReferenceElement spaceReferenceElement)
        {
            var spaceBindingList = coordinateManager.SpaceBindingList;
            foreach (var spaceBinding in spaceBindingList)
            {
                var success = BindSpace(spaceReferenceElement, spaceBinding);
                if (success)
                {
                    return true;
                }
            }
            return false;
        }

        private bool BindSpace(PomlSpaceReferenceElement spaceReferenceElement, SpaceBinding spaceBinding)
        {
            if (BindingIsValid(spaceReferenceElement, spaceBinding) == false)
            {
                return false;
            }

            bindingSpaceReferenceElement = spaceReferenceElement;
            spaceReferenceToObjectOrigin = GetSpaceReferenceToObjectOrigin(spaceReferenceElement);

            if (spaceBinding.Transform != null)
            {
                // If Transform is specified.
                bindingTransform = spaceBinding.Transform;
            }
            else
            {
                // If Pose is specified.
                var pose = spaceBinding.Pose;
                if (pose.HasValue)
                {
                    UpdatePosition(pose.Value.position, pose.Value.rotation);
                }
            }

            gameObject.SetActive(true);
            return true;
        }

        private static Matrix4x4 GetSpaceReferenceToObjectOrigin(PomlSpaceReferenceElement spaceReferenceElement)
        {
            var referencePosition = CoordinateUtility.ToUnityCoordinate(spaceReferenceElement.Position, directional: true);
            var referenceRotation = CoordinateUtility.ToUnityCoordinate(spaceReferenceElement.Rotation);

            var origin2binding = Matrix4x4.TRS(referencePosition, referenceRotation, Vector3.one);

            return origin2binding.inverse;
        }

        private static bool BindingIsValid(PomlSpaceReferenceElement spaceReferenceElement, SpaceBinding spaceBinding)
        {
            var spaceType = spaceReferenceElement.SpaceType;
            var spaceId = spaceReferenceElement.SpaceId;

            if (string.IsNullOrEmpty(spaceType) || string.Compare(spaceBinding.SpaceType, spaceType, true) == 0)
            {
                if (spaceBinding.SpaceId == spaceId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
