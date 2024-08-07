﻿using System;
using System.Collections.Generic;
using System.Linq;
using CustomShips.Helper;
using CustomShips.ZProperties;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Pieces {
    public class CustomShip : MonoBehaviour, IDestructible {
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
            Vector3 min = new Vector3(1000f, 1000f, 1000f);
            Vector3 max = new Vector3(-1000f, -1000f, -1000f);

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

                min = new Vector3(Mathf.Min(min.x, local.x), Mathf.Min(min.y, local.y), Mathf.Min(min.z, local.z));
                max = new Vector3(Mathf.Max(max.x, local.x), Mathf.Max(max.y, local.y), Mathf.Max(max.z, local.z));

                center += local;
                partCount++;

                buoyancyCenter += local * shipPart.buoyancy;
                buoyancySum += shipPart.buoyancy;
            }

            if (hasValidPart) {
                Vector3 size = new Vector3(Mathf.Abs(min.x - max.x), Mathf.Abs(min.y - max.y), Mathf.Abs(min.z - max.z)) + Vector3.one;

                ship.m_floatCollider.size = new Vector3(size.x, 0.2f, size.z);
                ship.m_floatCollider.transform.localPosition = buoyancyCenter / buoyancySum;
                rigidbody.centerOfMass = center / partCount;

                onboardTrigger.size = new Vector3(size.x + 1f, size.y + 3f, size.z + 1f);
                onboardTrigger.transform.position = rigidbody.worldCenterOfMass + new Vector3(0f, 1f, 0f);

                UpdateSteerForce(size.z);
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

        public void Damage(HitData hit) {
            if (shipParts.Count == 0) {
                return;
            }

            if (hit.m_hitType == HitData.HitType.AshlandsOcean) {
                int randomPart = UnityEngine.Random.Range(0, shipParts.Count);
                ShipPart part = shipParts[randomPart];
                TryDamage(part, hit);
            } else {
                ShipPart closestPart = ShipPart.FindNearest(this, hit.m_point);
                TryDamage(closestPart, hit);
            }
        }

        private bool TryDamage(ShipPart shipPart, HitData hit) {
            if (!shipPart) {
                return false;
            }

            if (!shipPart.TryGetComponent(out WearNTear wearNTear)) {
                Logger.LogWarning($"Ship part {shipPart.name} has no WearNTear component");
                return false;
            }

            wearNTear.Damage(hit);
            return true;
        }

        public DestructibleType GetDestructibleType() {
            return DestructibleType.Default;
        }
    }
}
