using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionAttachment : Attachment {
        // 設定
        [Serializable]
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition;
        }

        [SerializeField, Tooltip("追従設定")]
        private AttachmentSettings _settings = null;

        // 追従設定
        public AttachmentSettings Settings {
            get => _settings;
            set => _settings = value;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = _settings.space;
            // Position
            var offsetPosition = transform.position - GetTargetPosition();

            if (space == Space.Self) {
                offsetPosition = transform.InverseTransformVector(offsetPosition);
            }

            _settings.offsetPosition = offsetPosition;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetPosition = Vector3.zero;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offset = _settings.offsetPosition;

            if (space == Space.Self) {
                offset = transform.TransformVector(offset);
            }

            transform.position = GetTargetPosition() + offset;
        }
    }
}