using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// モーション制御用クラス
    /// </summary>
    public class MotionController : BodyController {
        private Animator _animator;

        public void SetMotion(int layerIndex, IMotionResolver motionResolver) {
            
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _animator = Body.GetComponent<Animator>();
        }
    }

    /// <summary>
    /// モーション再生に使用するPlayableを定義するためのインターフェース
    /// </summary>
    public interface IMotionResolver : IDisposable {
        // 再生に使用するPlayable
        IPlayable Playable { get; }
        // 再生時間
        float Duration { get; }
        // ループするか
        bool IsLoop { get; }
        // ループ開始オフセット
        float LoopOffset { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="playableGraph">初期化に使用するPlayableGraph</param>
        void Setup(ref PlayableGraph playableGraph);
        
        /// <summary>
        /// 再生時間の設定
        /// </summary>
        /// <param name="time">時間</param>
        void SetTime(float time);
    }
}