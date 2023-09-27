using System.Collections.Generic;
using UnityEngine;

namespace CustomShips {
    public class Rib : ShipPart {
        public float size = 2f;
        public Transform outerPoint;

        private static List<Rib> ribs = new List<Rib>();
        [SerializeField] private List<Transform> snapPoints = new List<Transform>();

        protected override void Awake() {
            base.Awake();
            ribs.Add(this);
            GetComponent<Piece>().GetSnapPoints(snapPoints);
        }

        private void OnDestroy() {
            ribs.Remove(this);
        }

        public static Rib FindRib(Vector3 position) {
            foreach (Rib rib in ribs) {
                foreach (Transform snapPoint in rib.snapPoints) {
                    Vector3 diff = snapPoint.position - position;

                    if (diff.magnitude <= 1f) {
                        return rib;
                    }
                }
            }

            return null;
        }
    }
}
