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

        public Rib leftRib;
        public Rib rightRib;

        protected override void Start() {
            base.Start();

            Vector3 position = transform.position;
            Vector3 right = transform.forward;
            Vector3 forward = -transform.right;

            leftRib = Rib.FindRib(position + right * -1f + forward * 0.5f);
            rightRib = Rib.FindRib(position + right * 1f + forward * 0.5f);

            if (leftRib && rightRib) {
                Vector3 start = (leftRib.transform.position + rightRib.transform.position) / 2f;
                Vector3 end = (leftRib.transform.TransformPoint(-leftRib.size, 0, 0) + rightRib.transform.TransformPoint(-rightRib.size, 0, 0)) / 2f;
                float angle = Mathf.Atan((leftRib.size - rightRib.size) / 2f) * Mathf.Rad2Deg;

                for (int i = 0; i < hullTargets.Length; i++) {
                    hullTargets[i].gameObject.SetActive(true);
                    hullTargets[i].position = Vector3.Lerp(start, end, (i + 1) / (float)hullTargets.Length) + hullOffsets[i];
                    hullTargets[i].localRotation = Quaternion.Euler(hullRotationAxes[i] * angle);
                }

                if (TryGetComponent(out DynamicHull dynamicHull)) {
                    dynamicHull.RegenerateMesh(leftRib.size + 0.1f, rightRib.size + 0.1f, 0.9f);
                }
            }
        }
    }
}
