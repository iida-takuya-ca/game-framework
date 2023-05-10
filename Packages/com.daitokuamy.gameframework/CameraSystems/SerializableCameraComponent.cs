using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// シリアライズ想定のカメラコンポーネント
    /// </summary>
    public abstract class SerializableCameraComponent<TCamera> : MonoBehaviour, ICameraComponent
        where TCamera : CinemachineVirtualCameraBase {
        [SerializeField, Tooltip("制御対象の仮想カメラ")]
        private TCamera _virtualCamera;

        // アクティブ状態
        bool ICameraComponent.IsActive => _virtualCamera != null && _virtualCamera.gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => _virtualCamera;
        // 仮想カメラ
        protected TCamera VirtualCamera => _virtualCamera;
        
        /// <summary>
        /// カメラアクティブ時処理
        /// </summary>
        void ICameraComponent.Activate() {
            if (_virtualCamera == null) {
                return;
            }

            if (((ICameraComponent)this).IsActive) {
                return;
            }

            _virtualCamera.gameObject.SetActive(true);
            ActivateInternal();
        }

        /// <summary>
        /// カメラ非アクティブ時処理
        /// </summary>
        void ICameraComponent.Deactivate() {
            if (_virtualCamera == null) {
                return;
            }

            if (!((ICameraComponent)this).IsActive) {
                return;
            }
            
            DeactivateInternal();
            _virtualCamera.gameObject.SetActive(false);
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        void ICameraComponent.Update(float deltaTime) {
            if (_virtualCamera == null) {
                return;
            }
            
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void ActivateInternal() {
        }
        
        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal(float deltaTime) {
        }
    }
}