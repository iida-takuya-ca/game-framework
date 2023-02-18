using System;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン管理用ハンドル
    /// </summary>
    public struct CollisionHandle : IDisposable {
        private bool _disposed;
        private CollisionManager _manager;

        // 有効なハンドルか
        internal bool IsValid => Key != null;

        // 管理用のキー
        internal object Key { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal CollisionHandle(CollisionManager manager, object key) {
            _disposed = false;
            _manager = manager;
            Key = key;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            _manager?.Unregister(this);
            Key = null;
        }
    }
}