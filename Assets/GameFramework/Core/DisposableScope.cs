using System;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public class DisposableScope : IScope, IDisposable {
        private bool _disposed;

        // スコープ終了通知
        public event Action OnExpired;

        /// <summary>
        /// クリア処理
        /// </summary>
        public void Clear() {
            if (_disposed) {
                return;
            }

            OnExpired?.Invoke();
            OnExpired = null;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            OnExpired?.Invoke();
            OnExpired = null;
        }
    }
}