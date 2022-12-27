using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationAttachment : Attachment {
        // 追従設定
        [Serializable]
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
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

            // Rotation
            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(GetTargetRotation()) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(GetTargetRotation());
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offset = Quaternion.Euler(_settings.offsetAngles);

            if (space == Space.Self) {
                transform.rotation = GetTargetRotation() * offset;
            }
            else {
                transform.rotation = offset * GetTargetRotation();
            }
        }
    }
}