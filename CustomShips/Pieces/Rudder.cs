using Jotunn;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Rudder : ShipPart {
        public Transform attachPoint;

        public void SetShipControls(ShipControlls shipControls) {
            shipControls.transform.position = transform.position;
            shipControls.transform.rotation = transform.rotation;

            shipControls.m_attachPoint.transform.position = attachPoint.position;
            shipControls.m_attachPoint.transform.rotation = attachPoint.rotation;

            foreach (BoxCollider boxCollider in shipControls.GetComponents<BoxCollider>()) {
                Destroy(boxCollider.gameObject);
            }

            foreach (BoxCollider boxCollider in gameObject.GetComponentsInChildren<BoxCollider>()) {
                BoxCollider newCollider = (BoxCollider)shipControls.gameObject.AddComponentCopy(boxCollider);
                newCollider.center += boxCollider.transform.position - transform.position;
                newCollider.size = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) * 0.9f;
            }
        }
    }
}
