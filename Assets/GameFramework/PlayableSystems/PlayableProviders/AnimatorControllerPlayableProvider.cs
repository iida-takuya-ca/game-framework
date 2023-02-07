using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のProvider
    /// </summary>
    public class AnimatorControllerPlayableProvider : PlayableProvider<AnimatorControllerPlayable> {
        private AnimatorControllerPlayable _playable;
        private RuntimeAnimatorController _controller;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="controller">再生対象のController</param>
        public AnimatorControllerPlayableProvider(RuntimeAnimatorController controller) {
            _controller = controller;
        }

        /// <summary>
        /// AnimatorController用のPlayableを取得
        /// </summary>
        public override AnimatorControllerPlayable GetPlayable() {
            return _playable;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            _playable = AnimatorControllerPlayable.Create(graph, _controller);
            return _playable;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _controller = null;
        }
    }
}