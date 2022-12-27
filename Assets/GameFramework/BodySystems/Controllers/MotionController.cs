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

        // ルートスケール制御用
        private RootScaleAnimationJobProvider _rootScaleAnimationJobProvider;

        // Animator
        public Animator Animator { get; private set; }
        // モーション再生用クラス
        public MotionPlayer Player { get; private set; }

        // ルートスケール（座標）
        public Vector3 RootPositionScale {
            get => _rootScaleAnimationJobProvider.PositionScale;
            set => _rootScaleAnimationJobProvider.PositionScale = value;
        }
        // ルートスケール（回転）
        public Vector3 RootAngleScale {
            get => _rootScaleAnimationJobProvider.AngleScale;
            set => _rootScaleAnimationJobProvider.AngleScale = value;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            Animator = Body.GetComponent<Animator>();
            Player = new MotionPlayer(Animator, _updateMode);

            // RootScaleJobの初期化
            _rootScaleAnimationJobProvider = new RootScaleAnimationJobProvider();
            Player.AddJob(_rootScaleAnimationJobProvider);
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