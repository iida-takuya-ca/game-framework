using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationRuntimeAttachment : RuntimeAttachment {
        // 追従設定
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
        }

        // 追従設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RotationRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public RotationRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;
            var offset = Quaternion.Euler(Settings.offsetAngles);

            if (space == Space.Self) {
                Owner.rotation = GetTargetRotation() * offset;
            }
            else {
                Owner.rotation = offset * GetTargetRotation();
            }
        }
    }
}