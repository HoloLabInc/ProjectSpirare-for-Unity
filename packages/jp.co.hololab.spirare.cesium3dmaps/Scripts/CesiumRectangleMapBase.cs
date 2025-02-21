using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class CesiumRectangleMapBase : MonoBehaviour
    {
        [SerializeField]
        private GameObject baseCube;

        [SerializeField]
        private AbstractCesiumRectangleMapCredit creditPrefab;

        private AbstractCesiumRectangleMapCredit creditFront;
        private AbstractCesiumRectangleMapCredit creditBack;
        private AbstractCesiumRectangleMapCredit creditLeft;
        private AbstractCesiumRectangleMapCredit creditRight;

        [SerializeField]
        private bool showCreditText = false;

        public bool ShowCreditText
        {
            get
            {
                return showCreditText;
            }
            set
            {
                showCreditText = value;

                var credits = new List<AbstractCesiumRectangleMapCredit>()
                {
                    creditFront, creditBack, creditLeft, creditRight
                };

                foreach (var credit in credits)
                {
                    credit.gameObject.SetActive(showCreditText);
                }
            }
        }

        private static readonly float creditTextSurfaceOffset = 0.001f;

        private void Awake()
        {
            if (creditPrefab != null)
            {
                ChangeCredit(creditPrefab);
            }
        }

        public void ChangeCredit(AbstractCesiumRectangleMapCredit creditPrefab)
        {
            var credits = new List<AbstractCesiumRectangleMapCredit>()
            {
                creditFront, creditBack, creditLeft, creditRight
            };

            foreach (var credit in credits)
            {
                if (credit != null)
                {
                    Destroy(credit.gameObject);
                }
            }

            creditFront = Instantiate(creditPrefab, transform);

            creditBack = Instantiate(creditPrefab, transform);
            creditBack.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);

            creditLeft = Instantiate(creditPrefab, transform);
            creditLeft.transform.localRotation = Quaternion.AngleAxis(90, Vector3.up);

            creditRight = Instantiate(creditPrefab, transform);
            creditRight.transform.localRotation = Quaternion.AngleAxis(270, Vector3.up);

            var baseCubeScale = baseCube.transform.localScale;
            UpdateCreditPosition(baseCubeScale.x, baseCubeScale.z);

            ShowCreditText = showCreditText;
        }

        public void ChangeSize(float x, float z)
        {
            // Change base cube scale
            var baseCubeScale = baseCube.transform.localScale;
            baseCubeScale.x = x;
            baseCubeScale.z = z;
            baseCube.transform.localScale = baseCubeScale;

            UpdateCreditPosition(x, z);
        }

        private void UpdateCreditPosition(float x, float z)
        {
            creditFront.transform.localPosition = new Vector3(0, 0, -(z / 2 + creditTextSurfaceOffset));
            creditFront.SetWidth(x);

            creditBack.transform.localPosition = new Vector3(0, 0, z / 2 + creditTextSurfaceOffset);
            creditBack.SetWidth(x);

            creditLeft.transform.localPosition = new Vector3(-(x / 2 + creditTextSurfaceOffset), 0, 0);
            creditLeft.SetWidth(z);

            creditRight.transform.localPosition = new Vector3(x / 2 + creditTextSurfaceOffset, 0, 0);
            creditRight.SetWidth(z);
        }
    }
}
