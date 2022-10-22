using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// 1つのClipを再生するPlayable用のHandler
    /// </summary>
    public class SingleMotionPlayableHandler : IMotionPlayableHandler
    {
        private AnimationClipPlayable _playable;
        private AnimationClip _clip;

        Playable IMotionPlayableHandler.Playable => _playable;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public SingleMotionPlayableHandler(AnimationClip clip)
        {
            _clip = clip;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose()
        {
            _clip = null;
            _playable.Destroy();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IMotionPlayableHandler.Initialize(PlayableGraph graph)
        {
            _playable = AnimationClipPlayable.Create(graph, _clip);
        }
    }
}