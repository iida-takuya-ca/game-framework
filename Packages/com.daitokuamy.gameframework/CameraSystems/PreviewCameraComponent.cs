using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// ビューワー用のカメラコンポーネント
    /// </summary>
    public class PreviewCameraComponent : SerializableCameraComponent<CinemachineVirtualCamera> {
        [SerializeField, Tooltip("距離")]
        private float _distance = 10.0f;
        [SerializeField, Tooltip("注視ターゲット")]
        private Transform _lookAt;

        // 角度
        private float _angleX;
        private float _angleY;
        // 注視点オフセット
        private Vector3 _lookAtOffset;
        
        // FOV
        public float Fov {
            get => VirtualCamera.m_Lens.FieldOfView;
            set => VirtualCamera.m_Lens.FieldOfView = value;
        }
        // 距離
        public float Distance {
            get => _distance;
            set => _distance = Mathf.Max(0.01f, value);
        }
        // X角度
        public float AngleX {
            get => _angleX;
            set => _angleX = Mathf.Clamp(value, -179.0f, 179.0f);
        }
        // Y角度
        public float AngleY {
            get => _angleY;
            set => _angleY = Mathf.Repeat(value, 360.0f);
        }
        // 注視対象
        public Transform LookAt {
            get => _lookAt;
            set => _lookAt = value;
        }
        // 注視点オフセット
        public Vector3 LookAtOffset {
            get => _lookAtOffset;
            set => _lookAtOffset = value;
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            // 距離と角度から姿勢を設定
            var basePosition = _lookAt != null ? _lookAt.position : Vector3.zero;
            var rotation = Quaternion.Euler(_angleX, _angleY, 0.0f);
            var relativePosition = rotation * Vector3.back * _distance;
            basePosition += rotation * _lookAtOffset;

            VirtualCamera.transform.position = basePosition + relativePosition;
            VirtualCamera.transform.LookAt(basePosition);
        }
    }
}