using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimRuntimeAttachment : RuntimeAttachment {
        // 追従設定
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

        // 追従設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AimRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public AimRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;
            var offsetRotation = Quaternion.Euler(Settings.offsetAngles);
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(Settings.forwardVector, Settings.upVector));
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) * axisRotation;

            if (space == Space.Self) {
                Owner.rotation = baseRotation * offsetRotation;
            }
            else {
                Owner.rotation = offsetRotation * baseRotation;
            }
        }
    }
}