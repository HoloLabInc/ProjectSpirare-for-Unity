using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Mrtk
{
    public class InteractionManagerMrtk : MonoBehaviour, IMixedRealityInputHandler
    {
        public MixedRealityInputAction SelectAction;

        private void OnEnable()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem.RegisterHandler<IMixedRealityInputHandler>(this);
        }

        private void OnDisable()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem.UnregisterHandler<IMixedRealityInputHandler>(this);
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (eventData.MixedRealityInputAction != SelectAction)
            {
                return;
            }

            var pointers = eventData.InputSource.Pointers;
            foreach (var pointer in pointers)
            {
                if (pointer.IsActive)
                {
                    if (pointer.Result != null && pointer.Result.CurrentPointerTarget != null)
                    {
                        var target = pointer.Result.CurrentPointerTarget;
                        var hitPomlElement = target.GetComponentInParent<PomlObjectElementComponent>();
                        if (hitPomlElement != null)
                        {
                            hitPomlElement.Select();
                        }
                    }
                }
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
        }
    }
}
