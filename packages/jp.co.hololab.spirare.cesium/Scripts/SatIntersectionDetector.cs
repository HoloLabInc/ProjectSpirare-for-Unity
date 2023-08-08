using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    internal static class SatIntersectionDetector
    {
        public struct Rect
        {
            public Vector2 Corner;
            public Vector2 Vector1;
            public Vector2 Vector2;
        }

        private struct Range
        {
            public float Min;
            public float Max;
        }

        public static bool Intersects(Rect rect1, Rect rect2)
        {
            var axes = new[] { rect1.Vector1, rect1.Vector2, rect2.Vector1, rect2.Vector2 };
            foreach (var axis in axes)
            {
                var intersect = IntersectWithAxis(rect1, rect2, axis);
                if (intersect == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IntersectWithAxis(Rect rect1, Rect rect2, Vector2 axis)
        {
            var range1 = ProjectOntoAxis(rect1, axis);
            var range2 = ProjectOntoAxis(rect2, axis);

            if (range1.Max < range2.Min || range2.Max < range1.Min)
            {
                return false;
            }

            return true;
        }

        private static Range ProjectOntoAxis(Rect rect, Vector2 axis)
        {
            var point1 = rect.Corner;
            var point2 = point1 + rect.Vector1;
            var point3 = point1 + rect.Vector2;
            var point4 = point1 + rect.Vector1 + rect.Vector2;
            var points = new[] { point1, point2, point3, point4 };

            var projections = points.Select(x => Vector2.Dot(x, axis)).ToArray();

            var min = Mathf.Min(projections);
            var max = Mathf.Max(projections);

            return new Range() { Max = max, Min = min };
        }
    }
}
