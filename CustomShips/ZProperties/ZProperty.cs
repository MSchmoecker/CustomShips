using UnityEngine;

namespace CustomShips.ZProperties {
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
            if (Application.isEditor) {
                Debug.LogWarning("Application running in Editor, skipping ZProperty.Set");
                return;
            }

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
}
