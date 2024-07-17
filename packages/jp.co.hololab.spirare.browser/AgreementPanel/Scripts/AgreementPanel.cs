using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser
{
    public class AgreementPanel : MonoBehaviour
    {
        [SerializeField]
        private Button agreeButton;

        [SerializeField]
        private Button disagreeButton;

        [SerializeField]
        private TMP_Text termsOfUseText;

        [SerializeField]
        private TextAsset licenseTextAsset;

        public bool HasAgreed
        {
            get
            {
                return PlayerPrefs.GetInt("HasAgreed", 0) == 1;
            }
        }

        private void Awake()
        {
            if (HasAgreed)
            {
                Destroy(gameObject);
                return;
            }

            agreeButton.onClick.AddListener(AgreeButton_OnClick);
            disagreeButton.onClick.AddListener(DisagreeButton_OnClick);
        }

        private void AgreeButton_OnClick()
        {
            throw new NotImplementedException();
        }

        private void DisagreeButton_OnClick()
        {
#if UNITY_EDITOR
            // stop app
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

