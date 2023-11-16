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

            if (TryGetComponent(out DynamicHull dynamicHull)) {
                if (leftRib && rightRib) {
                    dynamicHull.RegenerateMesh(leftRib.size + 0.1f, rightRib.size + 0.1f, 0.9f, 0, 0);
                } else if (leftRib) {
                    dynamicHull.RegenerateMesh(leftRib.size + 0.1f, 0.1f, 0.9f, 0, 0.4f);
                } else if (rightRib) {
                    dynamicHull.RegenerateMesh(0.1f, rightRib.size + 0.1f, 0.9f, 0.4f, 0);
                }
            }
        }
    }
}
