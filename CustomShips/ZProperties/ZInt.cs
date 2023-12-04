using UnityEngine;

namespace CustomShips.ZProperties {
    public class ZInt : ZProperty<int> {
        public ZInt(string key, int defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        public override int Get() {
            if (Application.isEditor) {
                Debug.LogWarning("Application running in Editor, returning default value for ZInt");
                return DefaultValue;
            }

            return NetView.GetZDO().GetInt(KeyHash, DefaultValue);
        }

        protected override void SetValue(int value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }
}
