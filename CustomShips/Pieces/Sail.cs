using System;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Sail : ShipPart {
        public float force = 0.03f;
        public Transform mast;
        public Transform sail;
        public Cloth sailCloth;

        private bool sailWasInPosition;

        private void Update() {
            if (!CustomShip) {
                return;
            }

            mast.transform.rotation = CustomShip.ship.m_mastObject.transform.rotation;
            sail.transform.localScale = CustomShip.ship.m_sailObject.transform.localScale;
            sailCloth.enabled = CustomShip.ship.m_sailCloth.enabled;

            bool sailInPosition = CustomShip.ship.m_sailWasInPosition;

            if (!sailWasInPosition && sailInPosition) {
                Utils.RecreateComponent(ref sailCloth);
            }

            sailWasInPosition = sailInPosition;
        }
    }
}
