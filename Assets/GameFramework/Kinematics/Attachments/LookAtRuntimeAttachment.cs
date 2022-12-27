using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtRuntimeAttachment : RuntimeAttachment {
        // 追従設定
        public class AttachmentSettings {
            [Tooltip("制御空間")]
            public Space space = Space.Self;
            [Tooltip("角度オフセット")]
            public Vector3 offsetAngles = Vector3.zero;
            [Tooltip("ねじり角度")]
            public float roll = 0.0f;
            [Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
            public Transform worldUpObject = null;
        }

        // 追従設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LookAtRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public LookAtRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;
            var offsetRotation = Quaternion.Euler(Settings.offsetAngles);
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) *
                Quaternion.Euler(0.0f, 0.0f, Settings.roll);

            if (space == Space.Self) {
                Owner.rotation = baseRotation * offsetRotation;
            }
            else {
                Owner.rotation = offsetRotation * baseRotation;
            }
        }
    }
}