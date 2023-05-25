using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class ScreenSpaceElementComponent : SpecificObjectElementComponentBase<PomlScreenSpaceElement>
    {
        protected override Task UpdateGameObjectCore()
        {
            UpdatePosition();
            return Task.CompletedTask;
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var targetLossyScale = CoordinateUtility.ToUnityCoordinate(element.Scale, directional: false);

            var parentScale = transform.parent.lossyScale;
            var targetLocalScale = new Vector3(targetLossyScale.x / parentScale.x, targetLossyScale.y / parentScale.y, targetLossyScale.z / parentScale.z);

            transform.localScale = targetLocalScale;

            transform.SetPositionAndRotation(
                CoordinateUtility.ToUnityCoordinate(element.Position, directional: true),
                CoordinateUtility.ToUnityCoordinate(element.Rotation));
            // transform.localScale = CoordinateUtility.ToUnityCoordinate(element.Scale, directional: false);
        }
    }
}
