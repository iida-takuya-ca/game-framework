using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentRuntimeAttachment : RuntimeAttachment {
        // Attachment設定
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        // Attachment設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ParentRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public ParentRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;
            var offsetPosition = Settings.offsetPosition;
            var rotation = Quaternion.Euler(Settings.offsetAngles);
            var offsetScale = Settings.offsetScale;

            if (space == Space.Self) {
                offsetPosition = Owner.TransformVector(offsetPosition);
                rotation = GetTargetRotation() * rotation;
            }
            else {
                rotation = rotation * GetTargetRotation();
            }

            if ((Settings.mask & TransformMasks.Position) != 0) {
                Owner.position = GetTargetPosition() + offsetPosition;
            }

            if ((Settings.mask & TransformMasks.Rotation) != 0) {
                Owner.rotation = rotation;
            }

            if ((Settings.mask & TransformMasks.Scale) != 0) {
                Owner.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
            }
        }
    }
}