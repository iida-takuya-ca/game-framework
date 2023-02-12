using GameFramework.PlayableSystems;
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
        private RootAnimationJobProvider _rootAnimationJobProvider;

        // Animator
        public Animator Animator { get; private set; }
        // モーション再生用クラス
        public MotionPlayer Player { get; private set; }

        // ルートスケール（座標）
        public Vector3 RootPositionScale {
            get => _rootAnimationJobProvider.PositionScale;
            set => _rootAnimationJobProvider.PositionScale = value;
        }
        // ルート速度オフセット
        public Vector3 RootVelocityOffset {
            get => _rootAnimationJobProvider.VelocityOffset;
            set => _rootAnimationJobProvider.VelocityOffset = value;
        }
        // ルートスケール（回転）
        public Vector3 RootAngleScale {
            get => _rootAnimationJobProvider.AngleScale;
            set => _rootAnimationJobProvider.AngleScale = value;
        }
        // ルート角速度オフセット
        public Vector3 RootAngularVelocityOffset {
            get => _rootAnimationJobProvider.AngularVelocityOffset;
            set => _rootAnimationJobProvider.AngularVelocityOffset = value;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            Animator = Body.GetComponent<Animator>();
            Player = new MotionPlayer(Animator, _updateMode);

            // RootScaleJobの初期化
            _rootAnimationJobProvider = new RootAnimationJobProvider();
            Player.JobConnector.SetProvider(_rootAnimationJobProvider);
            
            // TimeScale監視
            Body.LayeredTime.OnChangedTimeScale += scale => {
                Player.SetSpeed(scale);
            };
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            Player.Update();
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