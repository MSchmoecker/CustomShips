﻿using System;
using System.Collections.Generic;
using System.Linq;
using CustomShips.Helper;
using CustomShips.ZProperties;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Pieces {
    public class CustomShip : MonoBehaviour {
        public ShipControlls shipControlls;
        public BoxCollider floatCollider;
        public Transform partParent;
        public Transform forwardIndicator;
        public Transform rightIndicator;

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

            InvokeRepeating(nameof(UpdateRudder), 1, 1f);

            forwardIndicator.gameObject.SetActive(false);
            rightIndicator.gameObject.SetActive(false);
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

                    rudder.SetShipControls(shipControlls);
                    break;
                }
            }
        }

        private void FixedUpdate() {
            forwardIndicator.rotation = Quaternion.LookRotation(GetForward(), Vector3.up);
            rightIndicator.rotation = Quaternion.LookRotation(GetRight(), Vector3.up);
        }

        public static CustomShip FindCustomShip(int uniqueID) {
            CustomShip customShip = ships.FirstOrDefault(ship => ship && ship.uniqueID.Get() == uniqueID);
            return customShip;
        }

        public Vector3 GetForward() {
            if (currentRudder) {
                return currentRudder.transform.forward;
            }

            return transform.forward;
        }

        public Vector3 GetRight() {
            if (currentRudder) {
                return -currentRudder.transform.right;
            }

            return transform.right;
        }

        public Vector3 InverseTransformDirection(Vector3 windDir) {
            if (currentRudder) {
                return currentRudder.transform.InverseTransformDirection(windDir);
            }

            return transform.InverseTransformDirection(windDir);
        }
    }
}
