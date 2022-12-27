using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標コンストレイント
    /// </summary>
    public class PositionRuntimeConstraintExpression : RuntimeConstraintExpression, IJobPositionConstraint {
        // コンストレイント設定
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition;
        }

        // コンストレイント設定
        public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PositionRuntimeConstraintExpression(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public PositionRuntimeConstraintExpression(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        PositionConstraintJobHandle IJobPositionConstraint.CreateJobHandle(Animator animator) {
            var handle = new PositionConstraintJobHandle();
            handle.space = Settings.space;
            handle.offsetPosition = Settings.offsetPosition;
            handle.ownerHandle = animator.BindStreamTransform(Owner);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}