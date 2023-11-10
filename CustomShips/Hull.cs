using System;
using UnityEngine;

namespace CustomShips {
    public class Hull : ShipPart {
        [SerializeField]
        private HullSegment[] segments;

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

                foreach (HullSegment extension in segments) {
                    extension.target.gameObject.SetActive(true);
                    extension.target.position = center + transform.TransformDirection(extension.offset);
                    extension.target.localRotation = Quaternion.Euler(extension.rotationAxis * angle);
                }
            }
        }
    }
}
