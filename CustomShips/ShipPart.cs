using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips {
    public abstract class ShipPart : MonoBehaviour {
        private CustomShip customShip;

        public CustomShip CustomShip {
            get => customShip;
            set {
                customShip = value;
                if (customShip) {
                    customShip.AddPart(this);
                }
            }
        }

        public Collider mainCollider;

        protected List<Transform> snapPoints = new List<Transform>();

        private static Collider[] tmpColliders = new Collider[1000];

        protected virtual void Awake() {
            GetComponent<Piece>().GetSnapPoints(snapPoints);
        }

        protected virtual void Start() {
            if (!CustomShip) {
                CustomShip = GetComponentInParent<CustomShip>();
            }

            if (!CustomShip) {
                CreateCustomShip();
            }
        }

        private void CreateCustomShip() {
            Logger.LogInfo("Creating new ship");
            GameObject parent = Instantiate(Main.shipPrefab, transform.position, Quaternion.identity);
            CustomShip = parent.GetComponent<CustomShip>();
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
