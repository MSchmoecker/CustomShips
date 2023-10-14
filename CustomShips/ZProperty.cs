namespace CustomShips {
    public abstract class ZProperty<T> {
        public string Key { get; }

        public T DefaultValue { get; }

        protected ZNetView NetView { get; }

        protected int KeyHash { get; }

        private string RPCName { get; }

        protected ZProperty(string key, T defaultValue, ZNetView netView) {
            Key = key;
            RPCName = "RPC_" + key;
            KeyHash = Key.GetStableHashCode();
            DefaultValue = defaultValue;
            NetView = netView;
            NetView.Register<T>(RPCName, RPCSet);
        }

        public void Set(T value) {
            if (NetView.IsValid() && NetView.IsOwner()) {
                SetValue(value);
            } else {
                NetView.InvokeRPC(RPCName, value);
            }
        }

        public void Reset() {
            Set(DefaultValue);
        }

        public abstract T Get();

        protected abstract void SetValue(T value);

        private void RPCSet(long sender, T value) {
            SetValue(value);
        }
    }

    public class ZBool : ZProperty<bool> {
        public ZBool(string key, bool defaultValue, ZNetView zNetView) : base(key, defaultValue, zNetView) {
        }

        public override bool Get() {
            return NetView.GetZDO().GetBool(KeyHash, DefaultValue);
        }

        protected override void SetValue(bool value) {
            NetView.GetZDO().Set(KeyHash, value);
        }
    }

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
