using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標コンストレイント
    /// </summary>
    public class PositionRuntimeConstraint : RuntimeConstraint, IJobPositionConstraint {
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
        public PositionRuntimeConstraint(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public PositionRuntimeConstraint(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        PositionConstraintAnimationJob.Element IJobPositionConstraint.CreateJobElement(Animator animator) {
            var jobElement = new PositionConstraintAnimationJob.Element();
            jobElement.space = Settings.space;
            jobElement.offsetPosition = Settings.offsetPosition;
            jobElement.ownerHandle = animator.BindStreamTransform(Owner);
            jobElement.constraintAnimationJobParameter = CreateJobParameter(animator);
            return jobElement;
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