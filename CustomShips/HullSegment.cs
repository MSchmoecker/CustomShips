using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomShips {
    [Serializable]
    public class HullSegment {
        public Transform target;
        public Vector3 offset;
        public Vector3 rotationAxis;
    }
}
