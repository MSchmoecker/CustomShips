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
        private ZQuaternion localPartRotation;

        private void Awake() {
            nview = gameObject.GetComponent<ZNetView>();

            if (!Application.isEditor && (!nview || nview.GetZDO() == null)) {
                return;
            }

            ships.Add(this);

            ship = gameObject.GetComponent<Ship>();
            rigidbody = gameObject.GetComponent<Rigidbody>();

            uniqueID = new ZInt("MS_UniqueID", 0, nview);
            localPartRotation = new ZQuaternion("MS_LocalPartRotation", Quaternion.identity, nview);

            if (uniqueID.Get() == 0) {
                uniqueID.Set(Guid.NewGuid().ToString().GetStableHashCode());
            }

            InvokeRepeating(nameof(UpdatePieces), 1, 1f);
        }

        public void AddPart(ShipPart shipPart) {
            if (shipParts.Contains(shipPart)) {
                return;
            }

#if DEBUG
            Logger.LogInfo($"Adding part {shipPart.name} to ship");
#endif
            shipParts.Add(shipPart);
            shipPart.transform.SetParent(partParent);
        }

        private void UpdatePieces() {
            shipParts.RemoveAll(i => !i);
            UpdateRudder();
            UpdateSails();
            UpdateCollider();
            UpdateWeight();
        }

        public void UpdateRudder() {
            if (!Application.isEditor && !nview.IsOwner()) {
                return;
            }

            if (currentRudder) {
                currentRudder.SetShipControls(shipControls);
                return;
            }

            List<Rudder> rudders = GetPartsOfType<Rudder>();

            if (rudders.Count == 1) {
                currentRudder = rudders[0];

                Quaternion oldPartParentRotation = partParent.rotation;
                transform.rotation = Quaternion.LookRotation(currentRudder.transform.forward, currentRudder.transform.up);
                partParent.rotation = oldPartParentRotation;
                localPartRotation.Set(partParent.localRotation);

                currentRudder.SetShipControls(shipControls);
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

        private void Update() {
            if (localPartRotation != null) {
                partParent.localRotation = localPartRotation.Get();
            }
        }

        private void OnDestroy() {
            if (Application.isEditor) {
                return;
            }

            foreach (ShipPart shipPart in shipParts) {
                if (shipPart) {
                    shipPart.transform.SetParent(null);
                }
            }
        }

        private void UpdateCollider() {
            float minZ = 1000f;
            float maxZ = -1000f;
            float minX = 1000f;
            float maxX = -1000f;

            Vector3 center = Vector3.zero;
            int partCount = 0;
            Vector3 buoyancyCenter = Vector3.zero;
            float buoyancySum = 0;

            bool hasValidPart = false;

            foreach (ShipPart shipPart in shipParts) {
                if (!shipPart) {
                    continue;
                }

                hasValidPart = true;

                Vector3 local;

                if (shipPart is Rib rib) {
                    local = ToLocalPosition(rib.EndPosition);
                } else {
                    local = ToLocalPosition(shipPart);
                }

                minZ = Mathf.Min(minZ, local.z);
                maxZ = Mathf.Max(maxZ, local.z);

                minX = Mathf.Min(minX, local.x);
                maxX = Mathf.Max(maxX, local.x);

                center += local;
                partCount++;

                buoyancyCenter += local * shipPart.buoyancy;
                buoyancySum += shipPart.buoyancy;
            }

            if (hasValidPart) {
                float sizeZ = Mathf.Abs(minZ - maxZ) + 1f;
                float sizeX = Mathf.Abs(minX - maxX) + 1f;

                ship.m_floatCollider.size = new Vector3(sizeX, 0.2f, sizeZ);
                ship.m_floatCollider.transform.localPosition = buoyancyCenter / buoyancySum;
                rigidbody.centerOfMass = center / partCount;

                onboardTrigger.size = new Vector3(6f, 5f, sizeZ + 1f);
                onboardTrigger.transform.position = rigidbody.worldCenterOfMass;

                UpdateSteerForce(sizeZ);
            }
        }

        private void UpdateWeight() {
            rigidbody.mass = Mathf.Max(100, shipParts.Sum(part => part.Weight));
        }

        private void UpdateSteerForce(float sizeZ) {
            const float fromSize = 4f;
            const float toSize = 20f;
            float t = (sizeZ - fromSize) / (toSize - fromSize);

            ship.m_stearForce = Mathf.Lerp(0.2f, 1f, t);
            ship.m_stearVelForceFactor = Mathf.Lerp(0.2f, 0.8f, t);
            ship.m_backwardForce = Mathf.Lerp(0.5f, 0.2f, t);
            ship.m_angularDamping = Mathf.Lerp(0.05f, 0.3f, t);
        }

        public static CustomShip FindCustomShip(int uniqueID) {
            CustomShip customShip = ships.FirstOrDefault(ship => ship && ship.uniqueID.Get() == uniqueID);
            return customShip;
        }
    }
}
