using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionRuntimeAttachment : RuntimeAttachment {
        // 追従設定
        public class AttachmentSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition;
        }

        // 追従設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PositionRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public PositionRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;
            var offset = Settings.offsetPosition;

            if (space == Space.Self) {
                offset = Owner.TransformVector(offset);
            }

            Owner.position = GetTargetPosition() + offset;
        }
    }
}