using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomShips {
    public abstract class ShipPart : MonoBehaviour {
        public Collider mainCollider;

        protected List<Transform> snapPoints = new List<Transform>();

        private CustomShip customShip;
        private static Collider[] tmpColliders = new Collider[1000];

        protected virtual void Awake() {
            GetComponent<Piece>().GetSnapPoints(snapPoints);
        }

        protected virtual void Start() {
            int piecesOverlap = Physics.OverlapSphereNonAlloc(transform.position, 2f, tmpColliders, LayerMask.GetMask("piece"));

            for (int i = 0; i < piecesOverlap; i++) {
                ShipPart shipPart = tmpColliders[i].GetComponentInParent<ShipPart>();

                if (shipPart && shipPart.customShip) {
                    customShip = shipPart.customShip;
                    customShip.AddPart(this);
                }
            }

            if (!customShip) {
                customShip = GetComponentInParent<CustomShip>();
                customShip.AddPart(this);
            }

            if (!customShip) {
                CreateCustomShip();
            }
        }

        private void CreateCustomShip() {
            GameObject parent = Instantiate(Main.shipPrefab);
            customShip = parent.GetComponent<CustomShip>();
            customShip.AddPart(this);
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
