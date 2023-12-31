﻿using Jotunn;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Rudder : ShipPart, Interactable, Hoverable {
        public Transform rudder;
        public Transform attachPoint;

        private Rib rightRib;

        protected override void Start() {
            base.Start();
            InvokeRepeating(nameof(UpdateRudder), 0f, 3f);
        }

        private void Update() {
            if (CustomShip) {
                rudder.localRotation = CustomShip.ship.m_rudderObject.transform.localRotation;
            }
        }

        private void UpdateRudder() {
            Rib newRightRib = Rib.FindRib(transform.position + transform.right * 1f);

            if (newRightRib != rightRib) {
                rightRib = newRightRib;

                if (rudder && rightRib) {
                    Vector3 localRudderPosition = rudder.transform.localPosition;
                    localRudderPosition.x = rightRib.size + 0.2f;
                    rudder.transform.localPosition = localRudderPosition;
                }
            }
        }

        public void SetShipControls(ShipControlls shipControls) {
            shipControls.transform.position = transform.position;
            shipControls.transform.rotation = transform.rotation;

            shipControls.m_attachPoint.transform.position = attachPoint.position;
            shipControls.m_attachPoint.transform.rotation = attachPoint.rotation;

            shipControls.m_ship.m_stearForceOffset = CustomShip.ToLocalPosition(this).z;
        }

        public bool Interact(Humanoid user, bool hold, bool alt) {
            if (CustomShip.GetPartsOfType<Rudder>().Count > 1) {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$MS_CustomShips_MultipleRudder");
                return false;
            }

            return CustomShip.shipControls.Interact(user, hold, alt);
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            if (CustomShip.GetPartsOfType<Rudder>().Count > 1) {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$MS_CustomShips_MultipleRudder");
                return false;
            }

            return CustomShip.shipControls.UseItem(user, item);
        }

        public string GetHoverText() {
            return CustomShip.shipControls.GetHoverText();
        }

        public string GetHoverName() {
            return CustomShip.shipControls.GetHoverName();
        }
    }
}
