using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡大縮小追従
    /// </summary>
    public class ScaleRuntimeAttachment : RuntimeAttachment {
        // 追従設定
        public class AttachmentSettings {
            public Vector3 offsetScale = Vector3.one;
        }

        // 追従設定
        public AttachmentSettings Settings { get; set; } = new AttachmentSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScaleRuntimeAttachment(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public ScaleRuntimeAttachment(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            Owner.localScale = Vector3.Scale(GetTargetLocalScale(), Settings.offsetScale);
        }
    }
}