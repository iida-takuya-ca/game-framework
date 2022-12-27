using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentAttachment : Attachment {
        // 設定
        [Serializable]
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        [SerializeField, Tooltip("Attachment用設定")]
        private AttachmentSettings _settings = null;

        // コンストレイント設定
        public AttachmentSettings Settings {
            set => _settings = value;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetPosition = Vector3.zero;
            _settings.offsetAngles = Vector3.zero;
            _settings.offsetScale = Vector3.one;
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

            // Rotation
            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(GetTargetRotation()) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(GetTargetRotation());
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;

            // Scale
            var targetScale = GetTargetLocalScale();
            var localScale = transform.localScale;
            _settings.offsetScale = new Vector3
            (
                localScale.x / targetScale.x,
                localScale.y / targetScale.y,
                localScale.z / targetScale.z
            );
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offsetPosition = _settings.offsetPosition;
            var rotation = Quaternion.Euler(_settings.offsetAngles);
            var offsetScale = _settings.offsetScale;

            if (space == Space.Self) {
                offsetPosition = transform.TransformVector(offsetPosition);
                rotation = GetTargetRotation() * rotation;
            }
            else {
                rotation = rotation * GetTargetRotation();
            }

            if ((_settings.mask & TransformMasks.Position) != 0) {
                transform.position = GetTargetPosition() + offsetPosition;
            }

            if ((_settings.mask & TransformMasks.Rotation) != 0) {
                transform.rotation = rotation;
            }

            if ((_settings.mask & TransformMasks.Scale) != 0) {
                transform.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
            }
        }
    }
}