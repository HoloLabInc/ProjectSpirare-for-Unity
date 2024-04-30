using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser
{
    public class LicensePanel : MonoBehaviour
    {
        [SerializeField]
        private LicensePanelState licensePanelState;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TMP_Text licenseText;

        [SerializeField]
        private TextAsset licenseTextAsset;

        private bool initialized;

        private void Start()
        {
            LicensePanelState_OnIsPanelOpenChanged();
            licensePanelState.OnIsPanelOpenChanged += LicensePanelState_OnIsPanelOpenChanged;

            closeButton.onClick.AddListener(CloseButton_OnClick);
        }

        private void LicensePanelState_OnIsPanelOpenChanged()
        {
            if (licensePanelState.IsPanelOpen && initialized == false)
            {
                InitializeText();
                initialized = true;
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(licensePanelState.IsPanelOpen);
            }
        }

        private void CloseButton_OnClick()
        {
            licensePanelState.IsPanelOpen = false;
        }

        private void InitializeText()
        {
            var chunks = SplitTextIntoChunks(licenseTextAsset.text, 100);

            foreach (var chunk in chunks)
            {
                var chunkText = Instantiate(licenseText, licenseText.transform.parent);
                chunkText.text = chunk;
            }

            Destroy(licenseText.gameObject);
        }

        private static List<string> SplitTextIntoChunks(string text, int numberOfLines)
        {
            var chunks = new List<string>();
            var sb = new StringBuilder();

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(lines[i]);

                if ((i + 1) % numberOfLines == 0 || i == lines.Length - 1)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                }
            }

            return chunks;
        }
    }
}

