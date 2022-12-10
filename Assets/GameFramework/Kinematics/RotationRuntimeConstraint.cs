using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 回転コンストレイント
    /// </summary>
    public class RotationRuntimeConstraint : RuntimeConstraint {
        // コンストレイント設定
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
        }

        // コンストレイント設定
        public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RotationRuntimeConstraint(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public RotationRuntimeConstraint(Transform owner, Transform target)
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