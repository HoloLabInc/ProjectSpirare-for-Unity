using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;

namespace HoloLab.Spirare.Quest
{
    [RequireComponent(typeof(PomlObjectElementComponent))]
    public class OVRInteractableElementComponent : MonoBehaviour
    {
        [SerializeField]
        private List<InteractableUnityEventWrapper> interactableUnityEventWrappers = new List<InteractableUnityEventWrapper>();

        private PomlObjectElementComponent pomlObjectElementComponent;

        private void Start()
        {
            pomlObjectElementComponent = GetComponent<PomlObjectElementComponent>();

            foreach (var interactableUnityEventWrapper in interactableUnityEventWrappers)
            {
                interactableUnityEventWrapper.WhenSelect.AddListener(InteractableUnityEventWrapper_WhenSelect);
            }
        }

        private void OnDestroy()
        {
            foreach (var interactableUnityEventWrapper in interactableUnityEventWrappers)
            {
                interactableUnityEventWrapper.WhenSelect.RemoveListener(InteractableUnityEventWrapper_WhenSelect);
            }
        }

        private void InteractableUnityEventWrapper_WhenSelect()
        {
            if (pomlObjectElementComponent != null)
            {
                pomlObjectElementComponent.Select();
            }
        }
    }
}

