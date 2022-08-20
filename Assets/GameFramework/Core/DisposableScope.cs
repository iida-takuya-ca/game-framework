using System;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public class DisposableScope : IScope, IDisposable {
        // スコープ終了通知
        public event Action OnExpired;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            OnExpired?.Invoke();
            OnExpired = null;
        }
    }
}
