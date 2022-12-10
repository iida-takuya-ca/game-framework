using GameFramework.MotionSystems;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// モーション制御用クラス
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class MotionController : SerializedBodyController {
        [SerializeField, Tooltip("モーション更新モード")]
        private DirectorUpdateMode _updateMode = DirectorUpdateMode.GameTime;

        // モーション再生用クラス
        public MotionPlayer Player { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var animator = Body.GetComponent<Animator>();
            Player = new MotionPlayer(animator, _updateMode);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            Player.Update(deltaTime);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Player.Dispose();
        }

        /// <summary>
        /// 値変化通知
        /// </summary>
        private void OnValidate() {
            Player?.SetUpdateMode(_updateMode);
        }
    }
}