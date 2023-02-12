using UnityEngine;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// PlayablePlayer用の拡張メソッド
    /// </summary>
    public static class PlayablePlayerExtensions {
        /// <summary>
        /// モーションの設定
        /// </summary>
        public static AnimationClipPlayableProvider Change(this MotionPlayer source, AnimationClip clip,
            float blendDuration, bool autoDispose = true) {
            if (clip == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimationClipPlayableProvider(clip, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public static AnimatorControllerPlayableProvider Change(this MotionPlayer source,
            RuntimeAnimatorController controller,
            float blendDuration, bool autoDispose = true) {
            if (controller == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimatorControllerPlayableProvider(controller, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }
    }
}