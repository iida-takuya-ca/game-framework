using GameFramework.MotionSystems;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.BodySystems {
    /// <summary>
    /// モーション制御用クラス
    /// </summary>
    public class MotionController : BodyController {
        // モーション再生用クラス
        private MotionPlayer _player;

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(IMotionPlayableHandler handler, float blendDuration)
        {
            _player.SetMotion(handler, blendDuration);
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(AnimationClip clip, float blendDuration)
        {
            _player.SetMotion(clip, blendDuration);
        }
        
        /// <summary>
        /// Jobの追加
        /// </summary>
        public MotionPlayer.AnimationJobHandle AddJob<T>(IAnimationJobHandler<T> handler)
        where T : struct, IAnimationJob {
            return _player.AddJob(handler);
        }

        /// <summary>
        /// Jobの削除
        /// </summary>
        public void RemoveJob(MotionPlayer.AnimationJobHandle handle)
        {
            _player.RemoveJob(handle);
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var animator = Body.GetComponent<Animator>();
            _player = new MotionPlayer(animator);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            _player.Update(deltaTime);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _player.Dispose();
        }
    }
}