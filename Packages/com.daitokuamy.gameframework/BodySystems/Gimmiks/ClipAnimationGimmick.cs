using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// AnimationClipを使ったアニメーション再生ギミック
    /// </summary>
    public class ClipAnimationGimmick : AnimationGimmick {
        [SerializeField, Tooltip("再生させるAnimator")]
        private Animator _animator;
        [SerializeField, Tooltip("再生に使うAnimationClip")]
        private AnimationClip _animationClip;

        private PlayableGraph _graph;
        private AnimationClipPlayable _animationClipPlayable;

        // トータル時間
        public override float Duration => _animationClip != null ? _animationClip.length : 0.0f;
        // ループ再生するか
        public override bool IsLooping => _animationClip != null && _animationClip.isLooping;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _graph = PlayableGraph.Create($"AnimationGimmick_{_animationClip}");
            _animationClipPlayable = AnimationClipPlayable.Create(_graph, _animationClip);
            var output = AnimationPlayableOutput.Create(_graph, "output", _animator);
            output.SetSourcePlayable(_animationClipPlayable);
            _graph.Play();
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }

        /// <summary>
        /// 再生状態の反映
        /// </summary>
        protected override void Evaluate(float time) {
            _animationClipPlayable.SetTime(time);
            _graph.Evaluate(0.0f);
        }
    }
}