using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomShips {
    public abstract class ShipPart : MonoBehaviour {
        private CustomShip customShip;

        private static Collider[] tmpColliders = new Collider[1000];

        protected virtual void Awake() {
        }

        protected virtual void Start() {
            int piecesOverlap = Physics.OverlapSphereNonAlloc(transform.position, 2f, tmpColliders, LayerMask.GetMask("piece"));

            for (int i = 0; i < piecesOverlap; i++) {
                ShipPart shipPart = tmpColliders[i].GetComponentInParent<ShipPart>();

                if (shipPart && shipPart.customShip) {
                    customShip = shipPart.customShip;
                    transform.SetParent(shipPart.customShip.transform);
                }
            }

            if (!customShip) {
                CreateCustomShip();
            }
        }

        private void CreateCustomShip() {
            GameObject parent = new GameObject("CustomShip");
            customShip = parent.AddComponent<CustomShip>();
            transform.SetParent(parent.transform);
        }

        public static ShipPart FindNearest(Vector3 position) {
            int piecesOverlap = Physics.OverlapSphereNonAlloc(position, 2f, tmpColliders, LayerMask.GetMask("piece"));

            float minDistance = float.MaxValue;
            ShipPart nearest = null;

            for (int i = 0; i < piecesOverlap; i++) {
                ShipPart shipPart = tmpColliders[i].GetComponentInParent<ShipPart>();

                if (shipPart) {
                    float distance = Vector3.Distance(shipPart.transform.position, position);

                    if (distance < minDistance) {
                        minDistance = distance;
                        nearest = shipPart;
                    }
                }
            }

            return nearest;
        }
    }
}
