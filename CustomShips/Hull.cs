using System;
using UnityEngine;

namespace CustomShips {
    public class Hull : ShipPart {
        // Unity doesn't load serialized classes from dlls, thus we have to use individual arrays
        // https://issuetracker.unity3d.com/issues/assetbundle-is-not-loaded-correctly-when-they-reference-a-script-in-custom-dll-which-contains-system-dot-serializable-in-the-build
        [SerializeField]
        private Transform[] hullTargets;

        [SerializeField]
        private Vector3[] hullOffsets;

        [SerializeField]
        private Vector3[] hullRotationAxes;

        [SerializeField]
        private Rib leftRib;

        [SerializeField]
        private Rib rightRib;

        protected override void Start() {
            base.Start();

            Vector3 position = transform.position;
            Vector3 right = transform.forward;

            leftRib = Rib.FindRib(position + right * -1f);
            rightRib = Rib.FindRib(position + right * 1f);

            if (leftRib && rightRib) {
                Vector3 center = (leftRib.outerPoint.position + rightRib.outerPoint.position) / 2f;
                float angle = Mathf.Atan((leftRib.size - rightRib.size) / 2f) * Mathf.Rad2Deg;

                for (int i = 0; i < hullTargets.Length; i++) {
                    hullTargets[i].gameObject.SetActive(true);
                    hullTargets[i].position = center + transform.TransformDirection(hullOffsets[i]);
                    hullTargets[i].localRotation = Quaternion.Euler(hullRotationAxes[i] * angle);
                }
            }
        }
    }
}
