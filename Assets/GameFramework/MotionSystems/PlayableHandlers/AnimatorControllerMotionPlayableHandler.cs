using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のHandler
    /// </summary>
    public class AnimatorControllerMotionPlayableHandler : IMotionPlayableHandler
    {
        private AnimatorControllerPlayable _playable;
        private RuntimeAnimatorController _controller;

        Playable IMotionPlayableHandler.Playable => _playable;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public AnimatorControllerMotionPlayableHandler(RuntimeAnimatorController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose()
        {
            _controller = null;
            _playable.Destroy();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IMotionPlayableHandler.Initialize(PlayableGraph graph)
        {
            _playable = AnimatorControllerPlayable.Create(graph, _controller);
        }

        /// <summary>
        /// 時間の設定
        /// </summary>
        void IMotionPlayableHandler.SetTime(float time)
        {
            _playable.SetTime(time);
        }
    }
}