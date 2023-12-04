using System;
using System.Collections.Generic;
using System.Linq;
using CustomShips.ZProperties;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Pieces {
    public class CustomShip : MonoBehaviour {
        public ShipControlls shipControlls;

        [HideInInspector]
        public ZNetView nview;
        public ZInt uniqueID;

        private float floatForce = 700f;
        private const float waterLevelOffset = 0.2f;

        private static List<CustomShip> ships = new List<CustomShip>();
        private List<ShipPart> shipParts = new List<ShipPart>();
        private Rigidbody rigidbody;
        private Rudder currentRudder;

        private Dictionary<ShipPart, WaterVolume> previousWaterVolumes = new Dictionary<ShipPart, WaterVolume>();

        private void Awake() {
            ships.Add(this);
            rigidbody = gameObject.GetComponent<Rigidbody>();
            nview = gameObject.GetComponent<ZNetView>();
            uniqueID = new ZInt("MS_UniqueID", 0, nview);

            if (uniqueID.Get() == 0) {
                uniqueID.Set(Guid.NewGuid().ToString().GetStableHashCode());
            }

            InvokeRepeating(nameof(UpdateRudder), 1, 5f);
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

            Logger.LogInfo($"Adding part {shipPart.name} to ship");
            shipParts.Add(shipPart);
            shipPart.transform.SetParent(transform);
            rigidbody.mass = (shipParts.Count) * 10f;

            if (shipPart is Rudder) {
                UpdateRudder();
            }
        }

        public void UpdateRudder() {
            if (currentRudder) {
                return;
            }

            foreach (ShipPart shipPart in shipParts) {
                if (!shipPart) {
                    continue;
                }

                if (shipPart is Rudder rudder) {
                    currentRudder = rudder;
                    rudder.SetShipControls(shipControlls);
                    break;
                }
            }
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
