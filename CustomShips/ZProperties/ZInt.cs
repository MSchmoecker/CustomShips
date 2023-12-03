namespace CustomShips.ZProperties {
    public class ZInt : ZProperty<int> {
        public ZInt(string key, int defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        public override int Get() {
            return NetView.GetZDO().GetInt(KeyHash, DefaultValue);
        }

        protected override void SetValue(int value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }
}
