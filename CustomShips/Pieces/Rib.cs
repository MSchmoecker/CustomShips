using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Rib : ShipPart {
        public float size = 2f;

        public Rib leftRib;
        public Rib rightRib;

        public event Action OnChange;

        private static List<Rib> ribs = new List<Rib>();

        public Vector3 EndPosition => transform.position - transform.right * size;

        public override float Weight => weight * size;

        protected override void Awake() {
            base.Awake();
            ribs.Add(this);
            InvokeRepeating(nameof(UpdateRib), 0f, 3f);
        }

        private void OnDestroy() {
            ribs.Remove(this);
        }

        private void UpdateRib() {
            Vector3 position = transform.position;
            Vector3 right = transform.forward;
            Vector3 forward = -transform.right;

            Rib newLeftRib = FindRib(position + right * -2f + forward * 0.5f);
            Rib newRightRib = FindRib(position + right * 2f + forward * 0.5f);

            if (newLeftRib != leftRib || newRightRib != rightRib) {
                leftRib = newLeftRib;
                rightRib = newRightRib;

                OnChange?.Invoke();
            }
        }

        public static Rib FindRib(Vector3 position, bool ignoreY = true) {
            foreach (Rib rib in ribs) {
                Vector3 forward = -rib.transform.right;
                Vector3 diff = (rib.transform.position + forward * 0.5f) - position;

                if (ignoreY) {
                    diff.y = 0;
                }

                if (diff.magnitude <= 0.5f) {
                    return rib;
                }
            }

            return null;
        }

        private void OnDrawGizmos() {
            Vector3 forward = -transform.right;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + forward * 0.5f, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position - forward * 0.5f + forward * size, 0.1f);
        }
    }
}
