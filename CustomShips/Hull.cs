using System;
using UnityEngine;

namespace CustomShips {
    public class Hull : MonoBehaviour {
        [SerializeField]
        private Transform extension;

        [SerializeField]
        private Vector3 extensionStart;

        [SerializeField]
        private Vector3 extensionPositionAxis = Vector3.left;

        [SerializeField]
        private Vector3 extensionRotationAxis = Vector3.up;

        [SerializeField]
        private Rib leftRib;

        [SerializeField]
        private Rib rightRib;

        private void Start() {
            Vector3 position = transform.position;
            Vector3 right = transform.forward;

            leftRib = Rib.FindRib(position + right * -1f);
            rightRib = Rib.FindRib(position + right * 1f);

            if (leftRib && rightRib) {
                extension.gameObject.SetActive(true);

                float center = (leftRib.size + rightRib.size) / 2f;
                extension.localPosition = extensionStart + extensionPositionAxis * center;

                float angle = -Mathf.Atan((rightRib.size - leftRib.size) / 2f) * Mathf.Rad2Deg;
                extension.localRotation = Quaternion.Euler(extensionRotationAxis * angle);
            }
        }
    }
}
