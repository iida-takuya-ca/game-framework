using GameFramework.MotionSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body用の拡張メソッド
    /// </summary>
    public static class BodyExtensions {
        /// <summary>
        /// モーションの設定
        /// </summary>
        public static SingleMotionPlayableProvider
            SetMotion(this MotionPlayer source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (clip == null) {
                source.ResetMotion(blendDuration);
                return null;
            }

            var provider = new SingleMotionPlayableProvider(clip, autoDispose);
            source.SetMotion(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public static AnimatorControllerMotionPlayableProvider SetMotion(this MotionPlayer source,
            RuntimeAnimatorController controller,
            float blendDuration, bool autoDispose = true) {
            if (controller == null) {
                source.ResetMotion(blendDuration);
                return null;
            }

            var provider = new AnimatorControllerMotionPlayableProvider(controller, autoDispose);
            source.SetMotion(provider, blendDuration);
            return provider;
        }
    }
}