using UnityEngine;

namespace CustomShips.ZProperties {
    public class ZVector3 : ZProperty<Vector3> {
        public ZVector3(string key, Vector3 defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        protected override Vector3 GetValue() {
            return NetView.GetZDO().GetVec3(KeyHash, DefaultValue);
        }

        protected override void SetValue(Vector3 value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }
}
