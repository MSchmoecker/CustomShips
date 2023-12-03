using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips {
    public class CustomShip : MonoBehaviour {
        private float floatForce = 700f;
        private const float waterLevelOffset = 0.2f;

        private static List<CustomShip> ships = new List<CustomShip>();
        private List<ShipPart> shipParts = new List<ShipPart>();
        private Rigidbody rigidbody;
        public ZNetView nview;
        public ZInt uniqueID;

        private Dictionary<ShipPart, WaterVolume> previousWaterVolumes = new Dictionary<ShipPart, WaterVolume>();

        private void Awake() {
            ships.Add(this);
            rigidbody = gameObject.GetComponent<Rigidbody>();
            nview = gameObject.GetComponent<ZNetView>();
            uniqueID = new ZInt("MS_UniqueID", 0, nview);

            if (uniqueID.Get() == 0) {
                uniqueID.Set(Guid.NewGuid().ToString().GetStableHashCode());
            }
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

            Logger.LogInfo($"Adding part {shipPart.name} to ship");
            shipParts.Add(shipPart);
            shipPart.transform.SetParent(transform);
            rigidbody.mass = (shipParts.Count) * 10f;
        }

        private void FixedUpdate() {
            foreach (ShipPart shipPart in shipParts) {
                if (!shipPart) {
                    continue;
                }

                Vector3 position = shipPart.mainCollider.bounds.center;

                WaterVolume waterVolume = previousWaterVolumes.TryGetValue(shipPart, out WaterVolume volume) ? volume : null;
                float waterLevel = Floating.GetWaterLevel(position, ref waterVolume);
                previousWaterVolumes[shipPart] = waterVolume;

                if (position.y - waterLevelOffset > waterLevel) {
                    continue;
                }

                float distance = Mathf.Clamp01(Mathf.Abs(waterLevel - position.y));
                float force;

                if (position.y < waterLevel) {
                    force = (waterLevelOffset + distance) * floatForce;
                } else {
                    force = (waterLevelOffset - distance) * floatForce;
                }

                rigidbody.AddForceAtPosition(Vector3.up * force, position);
            }
        }

        public static CustomShip FindCustomShip(int uniqueID) {
            CustomShip customShip = ships.FirstOrDefault(ship => ship && ship.uniqueID.Get() == uniqueID);
            return customShip;
        }
    }
}
