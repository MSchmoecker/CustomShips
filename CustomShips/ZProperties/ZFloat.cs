using UnityEngine;

namespace CustomShips.ZProperties {
    public class ZFloat : ZProperty<float> {
        public ZFloat(string key, float defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        protected override float GetValue() {
            return NetView.GetZDO().GetFloat(KeyHash, DefaultValue);
        }

        protected override void SetValue(float value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }
}
