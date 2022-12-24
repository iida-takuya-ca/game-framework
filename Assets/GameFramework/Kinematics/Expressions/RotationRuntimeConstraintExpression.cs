using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 回転コンストレイント
    /// </summary>
    public class RotationRuntimeConstraintExpression : RuntimeConstraintExpression, IJobRotationConstraint {
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
        public RotationRuntimeConstraintExpression(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public RotationRuntimeConstraintExpression(Transform owner, Transform target)
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

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        RotationConstraintJobHandle IJobRotationConstraint.CreateJobHandle(Animator animator) {
            var handle = new RotationConstraintJobHandle();
            handle.space = Settings.space;
            handle.offsetRotation = quaternion.Euler(Settings.offsetAngles);
            handle.ownerHandle = animator.BindStreamTransform(Owner);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}