using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class ScreenSpaceElementComponent : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.position = Vector3.zero;
        }
    }
}
