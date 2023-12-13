using UnityEngine;

namespace CustomShips.ZProperties {
    public class ZQuaternion : ZProperty<Quaternion> {
        public ZQuaternion(string key, Quaternion defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        protected override Quaternion GetValue() {
            return NetView.GetZDO().GetQuaternion(KeyHash, DefaultValue);
        }

        protected override void SetValue(Quaternion value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }
}
