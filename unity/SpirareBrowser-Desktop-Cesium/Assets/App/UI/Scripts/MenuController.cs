using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.Desktop.Cesium
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField]
        private PlayerCharacterController characterController;

        [SerializeField]
        private RectTransform menuTransform;

        [SerializeField]
        private Button teleportButton;

        [SerializeField]
        private TMP_InputField destinationInputField;

        [SerializeField]
        private TMP_InputField moveSpeedInputField;

        [SerializeField]
        private TMP_InputField sprintSpeedInputField;

        [SerializeField]
        private TMP_InputField jumpHeightInputField;

        [SerializeField]
        private TMP_InputField respawnHeightInputField;

        [SerializeField]
        private Button resetParametersButton;

        [SerializeField]
        private bool menuOpen = false;

        private const float jumpHeightMax = 80;

        private const string destinationKey = "MenuController_Destination";
        private const string jumpHeightKey = "MenuController_JumpHeight";
        private const string moveSpeedKey = "MenuController_MoveSpeed";
        private const string sprintSpeedKey = "MenuController_SprintSpeed";
        private const string respawnHeightKey = "MenuController_RespawnHeight";

        private void Start()
        {
            StoreDefaultParameters();
            LoadSettings();

            ChangeMenuState(menuOpen);

            teleportButton.onClick.AddListener(Teleport);
            moveSpeedInputField.onEndEdit.AddListener(MoveSpeedInputField_OnEndEdit);
            sprintSpeedInputField.onEndEdit.AddListener(SprintSpeedInputField_OnEndEdit);
            jumpHeightInputField.onEndEdit.AddListener(JumpHeightInputField_OnEndEdit);

            respawnHeightInputField.onEndEdit.AddListener(RespawnHeightInputField_OnEndEdit);
            respawnHeightInputField.onDeselect.AddListener(RespawnHeightInputField_OnEndEdit);

            resetParametersButton.onClick.AddListener(ResetParametersButton_OnClick);
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleMenu();
            }
        }

        public void Teleport()
        {
            var destinationText = destinationInputField.text;
            if (TryParseLatitudeAndLongitude(destinationText, out var latitude, out double longitude))
            {
                Debug.Log($"Teleport to {latitude}, {longitude}");
                characterController.Teleport(latitude, longitude);
                SaveString(destinationKey, destinationText);
            }
            else
            {
                Debug.Log($"Failed to parse {destinationText}");
            }
        }


        #region Event handlers

        private void JumpHeightInputField_OnEndEdit(string jumpHeightString)
        {
            if (float.TryParse(jumpHeightString, out var jumpHeight))
            {
                ApplyJumpHeight(jumpHeight);
                SaveFloat(jumpHeightKey, jumpHeight);
            }
        }

        private void MoveSpeedInputField_OnEndEdit(string moveSpeedString)
        {
            if (float.TryParse(moveSpeedString, out var moveSpeed))
            {
                ApplyMoveSpeed(moveSpeed);
                SaveFloat(moveSpeedKey, moveSpeed);
            }
        }

        private void SprintSpeedInputField_OnEndEdit(string sprintSpeedString)
        {
            if (float.TryParse(sprintSpeedString, out var sprintSpeed))
            {
                ApplySprintSpeed(sprintSpeed);
                SaveFloat(sprintSpeedKey, sprintSpeed);
            }
        }

        private void RespawnHeightInputField_OnEndEdit(string respawnHeightString)
        {
            if (float.TryParse(respawnHeightString, out var respawnHeight))
            {
                ApplyRespawnHeight(respawnHeight);
                SaveFloat(respawnHeightKey, respawnHeight);
            }
        }

        private void ResetParametersButton_OnClick()
        {
            ApplyMoveSpeed(defaultParameters.MoveSpeed);
            SaveFloat(moveSpeedKey, defaultParameters.MoveSpeed);

            ApplySprintSpeed(defaultParameters.SprintSpeed);
            SaveFloat(sprintSpeedKey, defaultParameters.SprintSpeed);

            ApplyJumpHeight(defaultParameters.JumpHeight);
            SaveFloat(jumpHeightKey, defaultParameters.JumpHeight);

            ApplyRespawnHeight(defaultParameters.RespawnHeight);
            SaveFloat(respawnHeightKey, defaultParameters.RespawnHeight);
        }
        #endregion


        private void ApplyMoveSpeed(float moveSpeed)
        {
            moveSpeed = Mathf.Max(moveSpeed, 0);

            moveSpeedInputField.text = moveSpeed.ToString();
            characterController.MoveSpeed = moveSpeed;
        }

        private void ApplySprintSpeed(float sprintSpeed)
        {
            sprintSpeed = Mathf.Max(sprintSpeed, 0);

            sprintSpeedInputField.text = sprintSpeed.ToString();
            characterController.SprintSpeed = sprintSpeed;
        }

        private void ApplyJumpHeight(float jumpHeight)
        {
            jumpHeight = Mathf.Clamp(jumpHeight, 0, jumpHeightMax);

            jumpHeightInputField.text = jumpHeight.ToString();
            characterController.JumpHeight = jumpHeight;
        }

        private void ApplyRespawnHeight(float respawnHeight)
        {
            respawnHeight = Mathf.Max(respawnHeight, 0);

            respawnHeightInputField.text = respawnHeight.ToString();
            characterController.RespawnHeight = respawnHeight;
        }

        #region Initial loading

        private (float MoveSpeed, float SprintSpeed, float JumpHeight, float RespawnHeight) defaultParameters;

        private void StoreDefaultParameters()
        {
            var moveSpeed = characterController.MoveSpeed;
            var sprintSpeed = characterController.SprintSpeed;
            var jumpHeight = characterController.JumpHeight;
            var respawnHeight = characterController.RespawnHeight;

            defaultParameters = (moveSpeed, sprintSpeed, jumpHeight, respawnHeight);
        }

        private void LoadSettings()
        {
            LoadMoveSpeed();
            LoadSprintSpeed();
            LoadJumpHeight();
            LoadRespawnHeight();
            LoadDestination();
        }

        private void LoadMoveSpeed()
        {
            var moveSpeed = PlayerPrefs.GetFloat(moveSpeedKey, characterController.MoveSpeed);
            ApplyMoveSpeed(moveSpeed);
        }

        private void LoadSprintSpeed()
        {
            var sprintSpeed = PlayerPrefs.GetFloat(sprintSpeedKey, characterController.SprintSpeed);
            ApplySprintSpeed(sprintSpeed);
        }

        private void LoadJumpHeight()
        {
            var jumpHeight = PlayerPrefs.GetFloat(jumpHeightKey, characterController.JumpHeight);
            ApplyJumpHeight(jumpHeight);
        }

        private void LoadRespawnHeight()
        {
            var respawnHeight = PlayerPrefs.GetFloat(respawnHeightKey, characterController.RespawnHeight);
            ApplyRespawnHeight(respawnHeight);
        }

        private void LoadDestination()
        {
            var destination = PlayerPrefs.GetString(destinationKey);
            destinationInputField.text = destination;

            if (string.IsNullOrEmpty(destination) == false)
            {
                Teleport();
            }
        }
        #endregion

        private void ToggleMenu()
        {
            menuOpen = !menuOpen;

            ChangeMenuState(menuOpen);
        }

        private void ChangeMenuState(bool open)
        {
            menuTransform.gameObject.SetActive(open);

            var locked = !open;
            characterController.SetCursorInputForLook(locked);
        }

        private static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        private static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        private static bool TryParseLatitudeAndLongitude(string text, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            var tokens = text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                return false;
            }

            if (double.TryParse(tokens[0], out latitude) &&
                double.TryParse(tokens[1], out longitude))
            {
                return true;
            }

            return false;
        }
    }
}
