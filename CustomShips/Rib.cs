using System.Collections.Generic;
using UnityEngine;

namespace CustomShips {
    public class Rib : MonoBehaviour {
        public float size = 2f;

        private static List<Rib> ribs = new List<Rib>();

        private void Awake() {
            ribs.Add(this);
        }

        private void OnDestroy() {
            ribs.Remove(this);
        }

        public static Rib FindRib(Vector3 position) {
            foreach (Rib rib in ribs) {
                Vector3 diff = rib.transform.position - position;

                if (diff.magnitude < 0.1f) {
                    return rib;
                }
            }

            return null;
        }
    }
}
