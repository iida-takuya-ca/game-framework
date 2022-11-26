using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のProvider
    /// </summary>
    public class AnimatorControllerMotionPlayableProvider : MotionPlayableProvider
    {
        private AnimatorControllerPlayable _playable;
        private RuntimeAnimatorController _controller;

        protected override Playable Playable => _playable;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public AnimatorControllerMotionPlayableProvider(RuntimeAnimatorController controller, bool autoDispose)
            : base(autoDispose)
        {
            _controller = controller;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(PlayableGraph graph)
        {
            _playable = AnimatorControllerPlayable.Create(graph, _controller);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal()
        {
            _controller = null;
            _playable.Destroy();
        }
    }
}