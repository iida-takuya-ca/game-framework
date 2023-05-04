using Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 標準的なカメラコンポーネント
    /// </summary>
    public class DefaultCameraComponent : ICameraComponent {
        // アクティブ状態
        public bool IsActive => VirtualCamera != null && VirtualCamera.gameObject.activeSelf;

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
        /// アクティブ化
        /// </summary>
        void ICameraComponent.Activate() {
            if (VirtualCamera == null) {
                return;
            }

            if (IsActive) {
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

            if (!IsActive) {
                return;
            }
            
            VirtualCamera.gameObject.SetActive(false);
        }
    }
}