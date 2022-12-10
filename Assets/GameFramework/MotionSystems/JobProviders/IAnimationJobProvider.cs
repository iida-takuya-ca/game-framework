using System;
using UnityEngine.Animations;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// AnimationJob提供用インターフェース
    /// </summary>
    public interface IAnimationJobProvider<T> : IDisposable
        where T : struct, IAnimationJob {
        /// <summary>
        /// 実行順
        /// </summary>
        int ExecutionOrder { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="player">生成主のMotionController</param>
        /// <returns>生成したAnimationJob</returns>
        T Initialize(MotionPlayer player);
    }
}