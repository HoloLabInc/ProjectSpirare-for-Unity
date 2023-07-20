using CesiumForUnity;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Desktop.Cesium
{
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField]
        private float spawnPositionY = 0f;

        [SerializeField]
        private float resetPositionY = -50f;

        private CharacterController playerArmature;
        private ThirdPersonController thirdPersonController;
        private StarterAssetsInputs starterAssetsInputs;
        private CesiumGeoreference cesiumGeoreference;

        public float JumpHeight
        {
            get
            {
                return thirdPersonController.JumpHeight;
            }
            set
            {
                thirdPersonController.JumpHeight = value;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return thirdPersonController.MoveSpeed;
            }
            set
            {
                thirdPersonController.MoveSpeed = value;
            }
        }

        public float SprintSpeed
        {
            get
            {
                return thirdPersonController.SprintSpeed;
            }
            set
            {
                thirdPersonController.SprintSpeed = value;
            }
        }

        public float RespawnHeight
        {
            get
            {
                return spawnPositionY;
            }
            set
            {
                spawnPositionY = value;
            }
        }

        private void Awake()
        {
            starterAssetsInputs = GetComponentInChildren<StarterAssetsInputs>();
            playerArmature = GetComponentInChildren<CharacterController>();
            thirdPersonController = GetComponentInChildren<ThirdPersonController>();
            cesiumGeoreference = GetComponentInParent<CesiumGeoreference>();

            ResetPosition();
        }

        private void Update()
        {
            var position = playerArmature.transform.position;
            if (position.y < resetPositionY)
            {
                ResetPosition();
            }
        }

        public void Teleport(double latitude, double longitude)
        {
            var height = cesiumGeoreference.height;
            cesiumGeoreference.SetOriginLongitudeLatitudeHeight(longitude, latitude, height);
            MovePlayerArmature(new Vector3(0, spawnPositionY, 0));
        }

        public void ResetPosition()
        {
            var localPosition = playerArmature.transform.localPosition;
            localPosition.y = spawnPositionY;
            MovePlayerArmature(localPosition);
        }

        public void SetCursorInputForLook(bool locked)
        {
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.cursorLocked = locked;
                starterAssetsInputs.cursorInputForLook = locked;
            }

            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void MovePlayerArmature(Vector3 position)
        {
            playerArmature.enabled = false;
            playerArmature.transform.localPosition = position;
            playerArmature.enabled = true;
        }
    }
}
