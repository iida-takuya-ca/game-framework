using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playable提供クラスの基底
    /// </summary>
    public abstract class PlayableProvider<TPlayable> : IPlayableProvider
        where TPlayable : IPlayable {
        private Playable _playable;
        private bool _initialized;
        private bool _disposed;

        // 初期化済みか
        bool IPlayableProvider.IsInitialized => _initialized;
        // 廃棄済みか
        bool IPlayableProvider.IsDisposed => _disposed;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <inheritdoc/>
        void IPlayableProvider.Initialize(PlayableGraph graph) {
            if (!_initialized) {
                return;
            }
            
            _initialized = true;
            _playable = CreatePlayable(graph);
        }

        /// <summary>
        /// Playableの取得
        /// </summary>
        Playable IPlayableProvider.GetPlayable() => _playable;

        /// <summary>
        /// 再生時間の設定
        /// </summary>
        void IPlayableProvider.SetTime(float time) {
            _playable.SetTime(time);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IPlayableProvider.SetSpeed(float speed) {
            _playable.SetSpeed(speed);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            DisposeInternal();
        }

        /// <summary>
        /// 型指定してあるPlayableの取得
        /// </summary>
        public abstract TPlayable GetPlayable();

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected abstract Playable CreatePlayable(PlayableGraph graph);

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {}
    }
}