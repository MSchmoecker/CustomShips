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
        public BoxCollider onboardTrigger;
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
            rigidbody.mass = (shipParts.Count) * 30f;
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

                    Quaternion oldPartParentRotation = partParent.rotation;
                    transform.rotation = Quaternion.LookRotation(rudder.transform.forward, rudder.transform.up);
                    partParent.rotation = oldPartParentRotation;

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
                float sizeZ = Mathf.Abs(minZ - maxZ);

                ship.m_floatCollider.size = new Vector3(2f, 0.2f, sizeZ);
                ship.m_floatCollider.transform.position = rigidbody.worldCenterOfMass;

                onboardTrigger.size = new Vector3(6f, 5f, sizeZ + 1f);
                onboardTrigger.transform.position = rigidbody.worldCenterOfMass;
            }
        }

        public static CustomShip FindCustomShip(int uniqueID) {
            CustomShip customShip = ships.FirstOrDefault(ship => ship && ship.uniqueID.Get() == uniqueID);
            return customShip;
        }
    }
}
