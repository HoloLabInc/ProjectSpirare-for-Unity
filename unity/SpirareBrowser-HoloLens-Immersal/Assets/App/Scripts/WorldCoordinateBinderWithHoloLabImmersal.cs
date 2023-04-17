using HoloLab.Immersal;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.HoloLensImmersal
{
    public class WorldCoordinateBinderWithHoloLabImmersal : MonoBehaviour
    {
        private CoordinateManager coordinateManager;
        private ImmersalLocalization immersalLocalization;

        private void Start()
        {
            coordinateManager = CoordinateManager.Instance;
            immersalLocalization = GetComponentInChildren<ImmersalLocalization>();
            immersalLocalization.OnLocalized += ImmersalLocalization_OnLocalized;
        }

        private void ImmersalLocalization_OnLocalized(ImmersalLocalization.LocalizeInfo localizeInfo)
        {
            BindSpaceCoordinate(localizeInfo);
        }

        private void BindSpaceCoordinate(ImmersalLocalization.LocalizeInfo localizeInfo)
        {
            var mapOriginPose = localizeInfo.Pose.Inverse().GetTransformedBy(localizeInfo.CameraPose);
            var spaceType = SpaceOrigin.SpaceTypeImmersal;
            var mapId = localizeInfo.MapId.ToString();

            var spaceBinding = new SpaceBinding(mapOriginPose, spaceType, mapId);
            coordinateManager.BindSpace(spaceBinding);
        }
    }
}
