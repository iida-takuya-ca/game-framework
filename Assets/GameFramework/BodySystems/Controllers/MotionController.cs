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
        private RootMoveAnimationJobProvider _rootMoveAnimationJobProvider;

        // Animator
        public Animator Animator { get; private set; }
        // モーション再生用クラス
        public MotionPlayer Player { get; private set; }

        // ルートスケール（座標）
        public Vector3 RootPositionScale {
            get => _rootMoveAnimationJobProvider.PositionScale;
            set => _rootMoveAnimationJobProvider.PositionScale = value;
        }
        // ルート速度オフセット
        public Vector3 RootVelocityOffset {
            get => _rootMoveAnimationJobProvider.VelocityOffset;
            set => _rootMoveAnimationJobProvider.VelocityOffset = value;
        }
        // ルートスケール（回転）
        public Vector3 RootAngleScale {
            get => _rootMoveAnimationJobProvider.AngleScale;
            set => _rootMoveAnimationJobProvider.AngleScale = value;
        }
        // ルート角速度オフセット
        public Vector3 RootAngularVelocityOffset {
            get => _rootMoveAnimationJobProvider.AngularVelocityOffset;
            set => _rootMoveAnimationJobProvider.AngularVelocityOffset = value;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            Animator = Body.GetComponent<Animator>();
            Player = new MotionPlayer(Animator, _updateMode);

            // RootScaleJobの初期化
            _rootMoveAnimationJobProvider = new RootMoveAnimationJobProvider();
            Player.AddJob(_rootMoveAnimationJobProvider);
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