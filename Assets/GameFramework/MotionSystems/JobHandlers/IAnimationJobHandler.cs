using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// AnimationJob提供用インターフェース
    /// </summary>
    public interface IAnimationJobHandler<T> : IDisposable
    where T : struct, IAnimationJob
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="player">生成主のMotionController</param>
        /// <returns>生成したAnimationJob</returns>
        T Initialize(MotionPlayer player);
    }
}