using System.Collections.Generic;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Rib : ShipPart {
        public float size = 2f;

        private static List<Rib> ribs = new List<Rib>();

        protected override void Awake() {
            base.Awake();
            ribs.Add(this);
        }

        private void OnDestroy() {
            ribs.Remove(this);
        }

        public static Rib FindRib(Vector3 position) {
            foreach (Rib rib in ribs) {
                Vector3 forward = -rib.transform.right;
                Vector3 diff = (rib.transform.position + forward * 0.5f) - position;

                if (diff.magnitude <= 0.5f) {
                    return rib;
                }
            }

            return null;
        }
    }
}
