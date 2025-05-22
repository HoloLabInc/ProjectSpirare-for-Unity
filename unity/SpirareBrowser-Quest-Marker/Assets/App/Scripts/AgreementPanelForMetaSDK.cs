using HoloLab.Spirare.Browser;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Quest
{
    public class AgreementPanelForMetaSDK : MonoBehaviour
    {
        [SerializeField]
        private Toggle agreeButton;

        [SerializeField]
        private Toggle disagreeButton;

        [SerializeField]
        private TMP_Text agreementTmpText;

        [SerializeField]
        private AgreementSettings agreementSettings;

        public bool HasAgreed
        {
            get
            {
                if (TryGetHasAgreedKey(out var key))
                {
                    return PlayerPrefs.GetInt(key, 0) == 1;
                }
                else
                {
                    return false;
                }
            }
        }

        private void Awake()
        {
            if (HasAgreed || agreementSettings == null || string.IsNullOrEmpty(agreementSettings.AgreementText))
            {
                Destroy(gameObject);
                return;
            }

            agreementTmpText.text = agreementSettings.AgreementText;

            agreeButton.onValueChanged.AddListener(AgreeButton_OnValueChanged);
            disagreeButton.onValueChanged.AddListener(DisagreeButton_OnValueChanged);
        }

        private void AgreeButton_OnValueChanged(bool isToggle)
        {
            if (TryGetHasAgreedKey(out var key))
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }

            Destroy(gameObject);
        }

        private void DisagreeButton_OnValueChanged(bool isToggle)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool TryGetHasAgreedKey(out string key)
        {
            if (agreementSettings == null)
            {
                key = null;
                return false;
            }

            key = agreementSettings.PlayerPrefsKey;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

