using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 追従コンストレイント
    /// </summary>
    public class ParentRuntimeConstraintExpression : RuntimeConstraintExpression, IJobParentConstraint {
        // コンストレイント設定
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        // コンストレイント設定
        public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ParentRuntimeConstraintExpression(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public ParentRuntimeConstraintExpression(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ParentConstraintJobHandle IJobParentConstraint.CreateJobHandle(Animator animator) {
            var handle = new ParentConstraintJobHandle();
            handle.space = Settings.space;
            handle.masks = Settings.mask;
            handle.offsetPosition = Settings.offsetPosition;
            handle.offsetRotation = quaternion.EulerZXY(Settings.offsetAngles.z, Settings.offsetAngles.x,
                Settings.offsetAngles.y);
            handle.offsetScale = Settings.offsetScale;
            handle.ownerHandle = animator.BindStreamTransform(Owner);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}