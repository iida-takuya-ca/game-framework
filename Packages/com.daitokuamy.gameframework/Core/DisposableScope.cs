using System;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public class DisposableScope : IScope, IDisposable {
        // 廃棄済みか
        public bool Disposed { get; private set; }
        
        // スコープ終了通知
        public event Action OnExpired;

        /// <summary>
        /// クリア処理
        /// </summary>
        public void Clear() {
            if (Disposed) {
                return;
            }

            OnExpired?.Invoke();
            OnExpired = null;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (Disposed) {
                return;
            }

            Disposed = true;
            OnExpired?.Invoke();
            OnExpired = null;
        }
    }
}