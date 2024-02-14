using Cysharp.Threading.Tasks;
using HoloLab.PositioningTools.Immersal;
using HoloLab.PositioningTools.Immersal.UI;
using HoloLab.Spirare.Browser.EncryptedPrefs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Immersal
{
    public class ImmersalUIForSpirare : MonoBehaviour
    {
        [SerializeField]
        private ImmersalMapManager immersalMapManager = null;

        [SerializeField]
        private bool autoSignIn = true;

        private ImmersalSignInUI immersalSignInUI;

        private const string emailAddressKey = "ImmersalUIForSpirare_EmailAddress";
        private const string passwordKey = "ImmersalUIForSpirare_Password";

        private async void Start()
        {
            immersalSignInUI = GetComponentInChildren<ImmersalSignInUI>();
            LoadUserInfo();

            immersalMapManager.OnLogin += ImmersalMapManager_OnLogin;

            if (autoSignIn)
            {
                // Wait until Immersal UI is initialized
                await UniTask.Yield();

                if (string.IsNullOrEmpty(immersalSignInUI.EmailAddress) == false && string.IsNullOrEmpty(immersalSignInUI.Password) == false)
                {
                    await immersalSignInUI.LoginAsync();
                }
            }
        }

        private void ImmersalMapManager_OnLogin()
        {
            SaveUserInfo();
        }

        private void LoadUserInfo()
        {
            if (EncryptedPrefsManager.LoadString(emailAddressKey, out var emailAddress))
            {
                immersalSignInUI.EmailAddress = emailAddress;
            }

            if (EncryptedPrefsManager.LoadString(passwordKey, out var password))
            {
                immersalSignInUI.Password = password;
            }
        }

        private void SaveUserInfo()
        {
            EncryptedPrefsManager.SaveString(emailAddressKey, immersalSignInUI.EmailAddress);
            EncryptedPrefsManager.SaveString(passwordKey, immersalSignInUI.Password);
        }
    }
}
