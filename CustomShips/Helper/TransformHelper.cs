using UnityEngine;

namespace CustomShips.Helper {
    public static class TransformHelper {
        public static void SetRotationY(this Transform transform, float y) {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = y;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
