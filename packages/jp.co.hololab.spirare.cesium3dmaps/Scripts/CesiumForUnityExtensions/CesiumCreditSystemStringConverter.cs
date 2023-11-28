using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CesiumForUnity
{
    [RequireComponent(typeof(CesiumCreditSystem))]
    public class CesiumCreditSystemStringConverter : MonoBehaviour
    {
        private CesiumCreditSystem cesiumCreditSystem;

        public Action<string> OnCreditsUpdated;

        private void Start()
        {
            cesiumCreditSystem = GetComponent<CesiumCreditSystem>();
            cesiumCreditSystem.OnCreditsUpdate += CesiumCreditSystem_OnCreditsUpdate;
        }

        private void CesiumCreditSystem_OnCreditsUpdate(List<CesiumCredit> onScreenCredits, List<CesiumCredit> onPopupCredits)
        {
            var creditText = JoinCreditTexts(onScreenCredits);
            try
            {
                OnCreditsUpdated?.Invoke(creditText);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static string JoinCreditTexts(List<CesiumCredit> credits)
        {
            var components = credits.SelectMany(x => x.components);
            var componentTexts = components.Select(x => x.text);
            return string.Join(" ･ ", componentTexts);
        }
    }
}
