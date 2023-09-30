using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomShips {
    public class CustomShip : MonoBehaviour {
        private float floatForce = 500f;

        private List<ShipPart> shipParts = new List<ShipPart>();
        private Rigidbody rigidbody;

        private Dictionary<ShipPart, WaterVolume> previousWaterVolumes = new Dictionary<ShipPart, WaterVolume>();

        private void Awake() {
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.drag = 1f;
            rigidbody.angularDrag = 1f;
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

            shipParts.Add(shipPart);
            shipPart.transform.SetParent(transform);
            rigidbody.mass = (shipParts.Count) * 10f;
        }

        private void FixedUpdate() {
            foreach (ShipPart shipPart in shipParts) {
                Vector3 position = shipPart.mainCollider.bounds.center;

                WaterVolume waterVolume = previousWaterVolumes.TryGetValue(shipPart, out WaterVolume volume) ? volume : null;
                float waterLevel = Floating.GetWaterLevel(position, ref waterVolume);
                previousWaterVolumes[shipPart] = waterVolume;

                if (position.y - 0.5f > waterLevel) {
                    continue;
                }

                float distance = Mathf.Clamp01(Mathf.Abs(waterLevel - position.y));
                float force;

                if (position.y < waterLevel) {
                    force = (0.5f + distance) * floatForce;
                } else {
                    force = (0.5f - distance) * floatForce;
                }

                rigidbody.AddForceAtPosition(Vector3.up * force, position);
            }
        }
    }
}
