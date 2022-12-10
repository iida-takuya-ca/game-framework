using System;
using GameFramework.MotionSystems;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Playable用のProvider基底
    /// </summary>
    public abstract class MotionPlayableProvider : IMotionPlayableProvider {
        private bool _initialized;
        private readonly bool _autoDispose;

        bool IMotionPlayableProvider.AutoDispose => _autoDispose;
        Playable IMotionPlayableProvider.Playable => Playable;

        protected abstract Playable Playable { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public MotionPlayableProvider(bool autoDispose) {
            _autoDispose = autoDispose;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (_initialized) {
                DisposeInternal();
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IMotionPlayableProvider.Initialize(PlayableGraph graph) {
            if (_initialized) {
                return;
            }

            InitializeInternal(graph);
            _initialized = true;
        }

        /// <summary>
        /// 初期化処理(Override用)
        /// </summary>
        protected abstract void InitializeInternal(PlayableGraph graph);

        /// <summary>
        /// 解放処理(Override用)
        /// </summary>
        protected abstract void DisposeInternal();
    }
}