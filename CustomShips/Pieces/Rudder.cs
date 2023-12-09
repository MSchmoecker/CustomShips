using Jotunn;
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

            shipControls.m_ship.m_stearForceOffset = CustomShip.transform.InverseTransformPoint(transform.position).z;
        }

        public bool Interact(Humanoid user, bool hold, bool alt) {
            return CustomShip.shipControls.Interact(user, hold, alt);
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
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
