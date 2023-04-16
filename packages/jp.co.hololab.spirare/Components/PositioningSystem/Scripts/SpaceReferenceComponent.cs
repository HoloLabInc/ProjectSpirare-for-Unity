using HoloLab.PositioningTools.CoordinateSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLab.Spirare
{
    public sealed class SpaceReferenceComponent : SpaceOrigin
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(SpaceReferenceComponent))]
        private class SpacePlacementComponentEditor : SpaceOriginEditor
        {
        }
#endif
    }
}
