using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser
{
    public class PomlLoadContentsUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField contentsUrlInputField = null;

        [SerializeField]
        private Button loadContentsButton = null;

        [SerializeField]
        private TMP_Text loadMessageText = null;

        private PomlContentsManager contentsManager;

        private const string ContentsUrlInputFieldKey = "PomlLoadContentsUi_contntsUrl";

        private void Awake()
        {
            contentsManager = FindObjectOfType<PomlContentsManager>();

            loadContentsButton.onClick.AddListener(LoadContents);
            var contentsUrl = PlayerPrefs.GetString(ContentsUrlInputFieldKey);
            contentsUrlInputField.text = contentsUrl;

            contentsManager.OnStartLoadContent += ContentsManager_OnStartLoadContent;
        }

        private async void LoadContents()
        {
            var contentsUrl = contentsUrlInputField.text;

            PlayerPrefs.SetString(ContentsUrlInputFieldKey, contentsUrl);
            PlayerPrefs.Save();

            loadContentsButton.interactable = false;
            var result = await contentsManager.LoadContentsAsync(contentsUrl);
            loadContentsButton.interactable = true;

            var message = result.Success ? "Loaded successfully" : result.Error.Message;
            loadMessageText.text = message;
        }

        private void ContentsManager_OnStartLoadContent(string url)
        {
            // If content loading is executed, such as by a DeepLink,
            // change the URL entered in the InputField.
            if (url != contentsUrlInputField.text)
            {
                contentsUrlInputField.text = url;
                PlayerPrefs.SetString(ContentsUrlInputFieldKey, url);
                PlayerPrefs.Save();
            }
        }
    }
}
