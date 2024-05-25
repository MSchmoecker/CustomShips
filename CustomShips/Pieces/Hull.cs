using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomShips.Pieces {
    public class Hull : ShipPart {
        public MeshCollider sideCollider;
        public MeshFilter watermask;
        public Rib leftRib;
        public Rib rightRib;
        public Transform outerSnappoint;

        public float height = 0.9f;

        public event Action OnChange;

        protected override void Start() {
            base.Start();
            InvokeRepeating(nameof(UpdateHull), 0f, 3f);
        }

        public override float Weight => weight * Size;

        public float Size {
            get {
                if (leftRib && rightRib) {
                    return (leftRib.size + rightRib.size) / 2f;
                } else if (leftRib) {
                    return leftRib.size / 2f;
                } else if (rightRib) {
                    return rightRib.size / 2f;
                } else {
                    return 2f;
                }
            }
        }

        private void UpdateHull() {
            Vector3 position = transform.position;
            Vector3 right = transform.forward;
            Vector3 forward = -transform.right;

            Rib newLeftRib = Rib.FindRib(position + right * -1f + forward * 0.5f);
            Rib newRightRib = Rib.FindRib(position + right * 1f + forward * 0.5f);

            if (newLeftRib != leftRib || newRightRib != rightRib) {
                if (leftRib) leftRib.OnChange -= OnChange;
                if (rightRib) rightRib.OnChange -= OnChange;

                leftRib = newLeftRib;
                rightRib = newRightRib;

                if (leftRib) leftRib.OnChange += OnChange;
                if (rightRib) rightRib.OnChange += OnChange;

                float size;

                if (leftRib && rightRib) {
                    size = (leftRib.size + rightRib.size) / 2f;
                } else if (leftRib) {
                    size = leftRib.size / 2f;
                } else if (rightRib) {
                    size = rightRib.size / 2f;
                } else {
                    size = 2f;
                }

                outerSnappoint.localPosition = new Vector3(-size, height, 0);

                OnChange?.Invoke();
            }
        }

        private void OnDrawGizmos() {
            Vector3 position = transform.position;
            Vector3 right = transform.forward;
            Vector3 forward = -transform.right;

            Gizmos.DrawSphere(position + right * -1f + forward * 0.5f, 0.1f);
            Gizmos.DrawSphere(position + right * 1f + forward * 0.5f, 0.1f);
        }

        private void UpdateCollider() {
            Vector3 center;
            float size;
            float yRotation;

            if (leftRib && rightRib) {
                center = (GetRibCenter(leftRib, 0f) + GetRibCenter(rightRib, 0f)) / 2f;
                size = (leftRib.size + rightRib.size) / 2f;
                yRotation = Mathf.Atan((leftRib.size - rightRib.size) / 2f) * Mathf.Rad2Deg;
            } else if (leftRib) {
                center = (GetRibCenter(leftRib, 0f) + GetRibCenter(leftRib, 2f)) / 2f;
                size = leftRib.size;
                yRotation = Mathf.Atan((leftRib.size - 0f) / 2f) * Mathf.Rad2Deg;
            } else if (rightRib) {
                center = (GetRibCenter(rightRib, 0f) + GetRibCenter(rightRib, -2f)) / 2f;
                size = rightRib.size;
                yRotation = Mathf.Atan((0f - rightRib.size) / 2f) * Mathf.Rad2Deg;
            } else {
                center = transform.position;
                size = 2f;
                yRotation = 0;
            }

            float zRotation = Mathf.Atan(size / height) * Mathf.Rad2Deg - 90f;

            mainCollider.transform.position = center + Vector3.up * 0.35f;
            mainCollider.transform.localScale = new Vector3(size, 1f, 1f);
            mainCollider.transform.localRotation = Quaternion.Euler(0, yRotation, zRotation);
        }

        private Vector3 GetRibCenter(Rib rib, float offsetZ) {
            return rib.transform.TransformPoint(-rib.size / 2f, 0, offsetZ);
        }
    }
}
