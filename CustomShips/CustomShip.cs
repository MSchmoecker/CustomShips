using System;
using UnityEngine;

namespace CustomShips {
    public class CustomShip : MonoBehaviour {
        private void Awake() {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.mass = 25f;
        }
    }
}
