using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimAttachment : Attachment {
        // 追従設定
        [Serializable]
        public class AttachmentSettings {
            [Tooltip("制御空間")]
            public Space space = Space.Self;
            [Tooltip("角度オフセット")]
            public Vector3 offsetAngles = Vector3.zero;
            [Tooltip("正面のベクトル")]
            public Vector3 forwardVector = Vector3.forward;
            [Tooltip("上のベクトル")]
            public Vector3 upVector = Vector3.up;
            [Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
            public Transform worldUpObject = null;
        }
        
        [SerializeField, Tooltip("追従設定")]
        private AttachmentSettings _settings = null;

        // 追従設定
        public AttachmentSettings Settings {
            get => _settings;
            set => _settings = value;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetAngles = Vector3.zero;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = _settings.space;
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(_settings.forwardVector, _settings.upVector));
            var upVector = _settings.worldUpObject != null ? _settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) * axisRotation;

            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(baseRotation) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(baseRotation);
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offsetRotation = Quaternion.Euler(_settings.offsetAngles);
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(_settings.forwardVector, _settings.upVector));
            var upVector = _settings.worldUpObject != null ? _settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) * axisRotation;

            if (space == Space.Self) {
                transform.rotation = baseRotation * offsetRotation;
            }
            else {
                transform.rotation = offsetRotation * baseRotation;
            }
        }
    }
}