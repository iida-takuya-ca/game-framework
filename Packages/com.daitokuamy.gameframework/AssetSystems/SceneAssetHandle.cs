using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセットリクエスト用ハンドル
    /// </summary>
    public struct SceneAssetHandle : IProcess {
        // 無効なSceneAssetHandle
        public static readonly SceneAssetHandle Empty = new SceneAssetHandle();
        // IEnumerator用
        object IEnumerator.Current => null;

        // 読み込み情報
        private ISceneAssetInfo _info;

        // 読み込み完了しているか
        public bool IsDone => _info == null || _info.IsDone;
        // シーンインスタンス
        public SceneHolder SceneHolder => _info?.SceneHolder ?? new SceneHolder();
        // エラー
        public Exception Exception => _info?.Exception ?? null;
        // 有効なハンドルか
        public bool IsValid => _info != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public SceneAssetHandle(ISceneAssetInfo info) {
            _info = info;
        }

        /// <summary>
        /// 読み込んだアセットの解放
        /// </summary>
        internal void Release() {
            if (_info == null) {
                return;
            }

            _info.Dispose();
            _info = null;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}