using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のHandler
    /// </summary>
    public class AnimatorControllerMotionPlayableProvider : IMotionPlayableProvider
    {
        private AnimatorControllerPlayable _playable;
        private RuntimeAnimatorController _controller;

        Playable IMotionPlayableProvider.Playable => _playable;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public AnimatorControllerMotionPlayableProvider(RuntimeAnimatorController controller)
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
        void IMotionPlayableProvider.Initialize(PlayableGraph graph)
        {
            _playable = AnimatorControllerPlayable.Create(graph, _controller);
        }
    }
}