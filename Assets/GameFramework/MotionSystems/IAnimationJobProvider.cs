using System;
using UnityEngine.Animations;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// AnimationJob提供用インターフェース
    /// </summary>
    public interface IAnimationJobProvider : IDisposable {
        /// <summary>
        /// 実行順
        /// </summary>
        int ExecutionOrder { get; }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="playable">Jobを保持しているPlayable</param>
        /// <param name="deltaTime">変位時間</param>
        void Update(AnimationScriptPlayable playable, float deltaTime);
    }

    /// <summary>
    /// AnimationJob提供用インターフェース
    /// </summary>
    public interface IAnimationJobProvider<T> : IAnimationJobProvider
        where T : struct, IAnimationJob {
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="player">生成主のMotionController</param>
        /// <returns>生成したAnimationJob</returns>
        T Initialize(MotionPlayer player);
    }
}