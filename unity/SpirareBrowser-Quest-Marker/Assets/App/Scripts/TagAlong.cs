using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Quest
{
    public class TagAlong : MonoBehaviour
    {
        [SerializeField]
        private float distanceFromCamera = 1.0f;

        [SerializeField]
        private float lerpSpeed = 3.0f;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            var cameraTransform = mainCamera.transform;
            var targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
            transform.LookAt(cameraTransform.position);
            transform.Rotate(0, 180, 0);
        }
    }
}

