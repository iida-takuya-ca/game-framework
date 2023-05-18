using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// ビューワー用のカメラコンポーネント
    /// </summary>
    public class PreviewCameraComponent : SerializableCameraComponent<CinemachineVirtualCamera> {
        [SerializeField, Tooltip("距離")]
        private float _distance = 10.0f;
        [SerializeField, Tooltip("注視点オフセット")]
        private Vector3 _lookAtOffset;
        [SerializeField, Tooltip("X軸回転"), Range(-89.9f, 89.9f)]
        private float _angleX;
        [SerializeField, Tooltip("Y軸回転"), Range(0.0f, 360.0f)]
        private float _angleY;
        
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
            set => _angleX = Mathf.Clamp(value, -89.9f, 89.9f);
        }
        // Y角度
        public float AngleY {
            get => _angleY;
            set => _angleY = Mathf.Repeat(value, 360.0f);
        }
        // 注視対象
        public Transform LookAt {
            get => VirtualCamera.LookAt;
            set => VirtualCamera.LookAt = value;
        }
        // 注視点オフセット
        public Vector3 LookAtOffset {
            get => _lookAtOffset;
            set => _lookAtOffset = value;
        }
        // ニアクリップ
        public float NearClip {
            get => VirtualCamera.m_Lens.NearClipPlane;
            set => VirtualCamera.m_Lens.NearClipPlane = Mathf.Clamp(value, 0.01f, FarClip);
        }
        // ファークリップ
        public float FarClip {
            get => VirtualCamera.m_Lens.FarClipPlane;
            set => VirtualCamera.m_Lens.FarClipPlane = Mathf.Clamp(value, NearClip, float.MaxValue);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 基本的なコンポーネントは削除する
            VirtualCamera.DestroyCinemachineComponent<CinemachineComponentBase>();
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            var trans = VirtualCamera.transform;
            var lookAt = VirtualCamera.LookAt;
            
            // 相対位置計算
            var relativePosition = Quaternion.Euler(_angleX, _angleY, 0.0f) * Vector3.back * _distance;

            // LookAtを元に位置を計算
            var basePosition = lookAt != null ? lookAt.position : Vector3.zero;
            basePosition += Quaternion.Euler(0.0f, _angleY, 0.0f) * _lookAtOffset;
            
            // 姿勢に反映
            trans.position = basePosition + relativePosition;
            trans.LookAt(basePosition);
        }
    }
}