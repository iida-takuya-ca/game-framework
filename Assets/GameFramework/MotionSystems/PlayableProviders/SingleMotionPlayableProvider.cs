using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// 1つのClipを再生するPlayable用のProvider
    /// </summary>
    public class SingleMotionPlayableProvider : MotionPlayableProvider
    {
        private AnimationClipPlayable _playable;
        private AnimationClip _clip;

        protected override Playable Playable => _playable;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public SingleMotionPlayableProvider(AnimationClip clip, bool autoDispose)
            : base(autoDispose)
        {
            _clip = clip;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(PlayableGraph graph)
        {
            _playable = AnimationClipPlayable.Create(graph, _clip);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _clip = null;
            _playable.Destroy();
        }
    }
}