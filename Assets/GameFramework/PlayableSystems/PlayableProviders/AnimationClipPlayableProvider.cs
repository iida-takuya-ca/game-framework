using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimationClipを再生するPlayable用のProvider
    /// </summary>
    public class AnimationClipPlayableProvider : PlayableProvider<AnimationClipPlayable> {
        private AnimationClipPlayable _playable;
        private AnimationClip _clip;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clip">再生対象のAnimationClip</param>
        /// <param name="autoDispose">自動廃棄するか</param>
        public AnimationClipPlayableProvider(AnimationClip clip, bool autoDispose)
            : base(autoDispose) {
            _clip = clip;
        }

        /// <summary>
        /// AnimatorController用のPlayableを取得
        /// </summary>
        public override AnimationClipPlayable GetPlayable() {
            return _playable;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            _playable = AnimationClipPlayable.Create(graph, _clip);
            return _playable;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _clip = null;
        }
    }
}