using System;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// モーション再生用のPlayable提供インターフェース
    /// </summary>
    public interface IMotionPlayableProvider : IDisposable {
        // 自動Disposeフラグ
        bool AutoDispose { get; }
        // 再生に使用するPlayable
        Playable Playable { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(PlayableGraph graph);
    }
}