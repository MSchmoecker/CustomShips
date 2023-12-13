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

        [HideInInspector]
        public Ship ship;

        private static List<CustomShip> ships = new List<CustomShip>();

        private List<ShipPart> shipParts = new List<ShipPart>();
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

            InvokeRepeating(nameof(UpdatePieces), 1, 1f);
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

            Logger.LogInfo($"Adding part {shipPart.name} to ship");
            shipParts.Add(shipPart);
            shipPart.transform.SetParent(partParent);
            rigidbody.mass = (shipParts.Count) * 20f;
        }

        private void UpdatePieces() {
            UpdateRudder();
            UpdateSails();
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

        private void UpdateSails() {
            List<Sail> sails = GetPartsOfType<Sail>();

            float forceSum = sails.Sum(sail => sail.force);
            float weightedForce = Mathf.Round(1000f * (Mathf.Log(1 + forceSum * 4f) / 4f)) / 1000f;
            // Vector3 centerOfForce = sails.Aggregate(Vector3.zero, (current, sail) => current + ToLocalPosition(sail)) / sails.Count;

            ship.m_sailForceFactor = weightedForce;
        }

        public List<T> GetPartsOfType<T>() where T : ShipPart {
            return shipParts.Select(part => part as T).Where(part => part).ToList();
        }

        public Vector3 ToLocalPosition(Vector3 global) {
            return transform.InverseTransformPoint(global);
        }

        public Vector3 ToLocalPosition(ShipPart part) {
            return ToLocalPosition(part.transform.position);
        }

        private void FixedUpdate() {
            float minZ = 1000f;
            float maxZ = -1000f;
            float minX = 1000f;
            float maxX = -1000f;

            bool hasValidPart = false;

            foreach (ShipPart shipPart in shipParts) {
                if (!shipPart) {
                    continue;
                }

                hasValidPart = true;
                Vector3 local = ToLocalPosition(shipPart);
                minZ = Mathf.Min(minZ, local.z);
                maxZ = Mathf.Max(maxZ, local.z);

                if (shipPart is Rib rib) {
                    local = ToLocalPosition(rib.EndPosition);
                }

                minX = Mathf.Min(minX, local.x);
                maxX = Mathf.Max(maxX, local.x);

            }

            if (hasValidPart) {
                float sizeZ = Mathf.Max(1f, Mathf.Abs(minZ - maxZ));
                float sizeX = Mathf.Max(1f, Mathf.Abs(minX - maxX));

                ship.m_floatCollider.size = new Vector3(sizeX, 0.2f, sizeZ);
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
