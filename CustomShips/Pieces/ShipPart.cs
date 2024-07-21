using System;
using System.Collections.Generic;
using CustomShips.ZProperties;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Pieces {
    public abstract class ShipPart : MonoBehaviour {
        public float weight = 5;
        public float buoyancy = 1;
        private CustomShip customShip;
        private ZNetView nview;
        private ZInt connectedShip;

        public CustomShip CustomShip {
            get => customShip;
            set {
                customShip = value;

                if (customShip) {
                    customShip.AddPart(this);
                    connectedShip.Set(customShip.uniqueID.Get());

                    if (Application.isEditor) {
                        Debug.LogWarning("Application running in Editor, skipping setting CustomShip");
                        return;
                    }

                    if (!nview.GetZDO().GetBool("MS_HasRelativePosition")) {
                        nview.GetZDO().Set(ZDOVars.s_relPosHash, transform.localPosition);
                        nview.GetZDO().Set(ZDOVars.s_relRotHash, transform.localRotation);
                        nview.GetZDO().Set("MS_HasRelativePosition", true);
                    } else {
                        nview.GetZDO().GetVec3(ZDOVars.s_relPosHash, out Vector3 relPos);
                        nview.GetZDO().GetQuaternion(ZDOVars.s_relRotHash, out Quaternion relRot);
                        transform.localPosition = relPos;
                        transform.localRotation = relRot;
                    }
                }
            }
        }

        public Vector3 Forward => transform.right;

        public Collider mainCollider;

        protected List<Transform> snapPoints = new List<Transform>();

        private static Collider[] tmpColliders = new Collider[1000];

        protected virtual void Awake() {
            GetComponent<Piece>().GetSnapPoints(snapPoints);
            nview = GetComponent<ZNetView>();
            connectedShip = new ZInt("MS_ConnectedShip", 0, nview);
        }

        protected virtual void Start() {
            InvokeRepeating(nameof(SearchShip), 0, 3);
            InvokeRepeating(nameof(SetPosition), 0, 5);
        }

        private void SearchShip() {
            if (CustomShip) {
                return;
            }

            CustomShip = transform.GetComponentInParent<CustomShip>();

            if (CustomShip) {
                return;
            }

            int connectedShipId = connectedShip.Get();
            CustomShip shipInstance = CustomShip.FindCustomShip(connectedShipId);

            if (shipInstance) {
                CustomShip = shipInstance;
            }
        }

        public virtual float Weight => weight;

        private void SetPosition() {
            if (nview.IsOwner()) {
                // only for updating the zone position to stay close to the ship
                nview.GetZDO().SetPosition(transform.position);
            }
        }

        public void CreateCustomShip() {
#if DEBUG
            Logger.LogInfo("Creating new ship");
#endif
            GameObject parent = Instantiate(Main.customShip.PiecePrefab, transform.position, Quaternion.identity);
            CustomShip = parent.GetComponent<CustomShip>();
        }

        public static ShipPart FindNearest(CustomShip ship, Vector3 position) {
            int piecesOverlap = Physics.OverlapSphereNonAlloc(position, 4f, tmpColliders, LayerMask.GetMask("piece"));

            float minDistance = float.MaxValue;
            ShipPart nearest = null;

            for (int i = 0; i < piecesOverlap; i++) {
                ShipPart shipPart = tmpColliders[i].GetComponentInParent<ShipPart>();

                if (!shipPart || !shipPart.CustomShip) {
                    continue;
                }

                if (ship && shipPart.CustomShip != ship) {
                    continue;
                }

                float distance = Vector3.Distance(shipPart.transform.position, position);

                if (distance < minDistance) {
                    minDistance = distance;
                    nearest = shipPart;
                }
            }

            return nearest;
        }
    }
}
