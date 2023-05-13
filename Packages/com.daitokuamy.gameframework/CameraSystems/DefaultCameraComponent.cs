using Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 標準的なカメラコンポーネント
    /// </summary>
    public class DefaultCameraComponent : ICameraComponent {
        // アクティブ状態
        bool ICameraComponent.IsActive => VirtualCamera != null && VirtualCamera.gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => VirtualCamera;

        // 制御対象の仮想カメラ
        public CinemachineVirtualCameraBase VirtualCamera { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualCamera">制御対象の仮想カメラ</param>
        public DefaultCameraComponent(CinemachineVirtualCameraBase virtualCamera) {
            VirtualCamera = virtualCamera;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICameraComponent.Initialize(){
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        void ICameraComponent.Activate() {
            if (VirtualCamera == null) {
                return;
            }

            if (((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(true);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void ICameraComponent.Deactivate() {
            if (VirtualCamera == null) {
                return;
            }

            if (!((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新
        /// </summary>
        void ICameraComponent.Update(float deltaTime) {
        }
    }
}