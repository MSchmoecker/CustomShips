using System;
using System.Collections.Generic;
using System.Linq;
using CustomShips.Helper;
using CustomShips.ZProperties;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Pieces {
    public class CustomShip : MonoBehaviour {
        public ShipControlls shipControls;
        public Transform partParent;
        public ZInt uniqueID;

        private static List<CustomShip> ships = new List<CustomShip>();

        private List<ShipPart> shipParts = new List<ShipPart>();
        private Ship ship;
        private ZNetView nview;
        private Rigidbody rigidbody;
        private Rudder currentRudder;

        private void Awake() {
            ships.Add(this);
            ship = gameObject.GetComponent<Ship>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            nview = gameObject.GetComponent<ZNetView>();
            uniqueID = new ZInt("MS_UniqueID", 0, nview);

            if (uniqueID.Get() == 0) {
                uniqueID.Set(Guid.NewGuid().ToString().GetStableHashCode());
            }

            InvokeRepeating(nameof(UpdateRudder), 1, 1f);
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

            Logger.LogInfo($"Adding part {shipPart.name} to ship");
            shipParts.Add(shipPart);
            shipPart.transform.SetParent(partParent);
            rigidbody.mass = (shipParts.Count) * 10f;
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

                    float partParentRotationY = partParent.rotation.eulerAngles.y;
                    float rotationY = Quaternion.LookRotation(rudder.transform.forward, Vector3.up).eulerAngles.y;

                    transform.SetRotationY(rotationY);
                    partParent.SetRotationY(partParentRotationY - partParent.rotation.y);

                    rudder.SetShipControls(shipControls);
                    break;
                }
            }
        }

        private void FixedUpdate() {
            float minZ = 1000f;
            float maxZ = -1000f;

            bool hasValidPart = false;

            foreach (ShipPart shipPart in shipParts) {
                if (!shipPart) {
                    continue;
                }

                hasValidPart = true;
                float z = transform.InverseTransformPoint(shipPart.transform.position).z;
                minZ = Mathf.Min(minZ, z);
                maxZ = Mathf.Max(maxZ, z);
            }

            if (hasValidPart) {
                Vector3 floatColliderSize = ship.m_floatCollider.size;
                floatColliderSize.z = Mathf.Abs(minZ - maxZ) + 2;
                ship.m_floatCollider.size = floatColliderSize;
                ship.m_floatCollider.transform.localPosition = new Vector3(0, 0, (minZ + maxZ) / 2f);
            }
        }

        public static CustomShip FindCustomShip(int uniqueID) {
            CustomShip customShip = ships.FirstOrDefault(ship => ship && ship.uniqueID.Get() == uniqueID);
            return customShip;
        }
    }
}
