using System;
using UnityEngine;

namespace HoloLab.Spirare
{
    public static class CoordinateUtility
    {
        public static Vector3 ToSpirareCoordinate(Vector3 vector3, bool directional = true)
        {
            if (directional)
            {
                return new Vector3(vector3.z, -vector3.x, vector3.y);
            }
            else
            {
                return new Vector3(vector3.z, vector3.x, vector3.y);
            }
        }

        public static Vector3 ToUnityCoordinate(float x, float y, float z, bool directional = true)
        {
            if (directional)
            {
                return new Vector3(-y, z, x);
            }
            else
            {
                return new Vector3(y, z, x);
            }
        }

        public static Vector3 ToUnityCoordinate(Vector3 position, bool directional = true)
        {
            return ToUnityCoordinate(position.x, position.y, position.z, directional);
        }

        public static Quaternion ToSpirareCoordinate(Quaternion rotation)
        {
            return new Quaternion(-rotation.z, rotation.x, -rotation.y, rotation.w);
        }

        public static Quaternion ToUnityCoordinate(float x, float y, float z, float w)
        {
            return new Quaternion(-y, z, x, -w);
        }

        public static Quaternion ToUnityCoordinate(Quaternion rotation)
        {
            return ToUnityCoordinate(rotation.x, rotation.y, rotation.z, rotation.w);
        }
    }
}
