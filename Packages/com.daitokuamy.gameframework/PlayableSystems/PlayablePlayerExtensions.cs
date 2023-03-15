using UnityEngine;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// PlayablePlayer用の拡張メソッド
    /// </summary>
    public static class PlayablePlayerExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableProvider Change(this MotionPlayer source, int layerIndex, AnimationClip clip,
            float blendDuration, bool autoDispose = true) {
            if (clip == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimationClipPlayableProvider(clip, autoDispose);
            source.Change(layerIndex, provider, blendDuration);
            return provider;
        }
        public static AnimationClipPlayableProvider Change(this MotionPlayer source, AnimationClip clip,
            float blendDuration, bool autoDispose = true) {
            return source.Change(0, clip, blendDuration, autoDispose);
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayableProvider Change(this MotionPlayer source, int layerIndex,
            RuntimeAnimatorController controller,
            float blendDuration, bool autoDispose = true) {
            if (controller == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimatorControllerPlayableProvider(controller, autoDispose);
            source.Change(layerIndex, provider, blendDuration);
            return provider;
        }
        public static AnimatorControllerPlayableProvider Change(this MotionPlayer source,
            RuntimeAnimatorController controller,
            float blendDuration, bool autoDispose = true) {
            return source.Change(0, controller, blendDuration, autoDispose);
        }
    }
}