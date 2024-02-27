using UnityEngine;

namespace CesiumForUnity
{
    public class AddCesiumCreditSystemAtRuntime : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.AddComponent<CesiumCreditSystem>();
        }
    }
}
