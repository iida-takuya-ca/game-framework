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
        }

        // コンストレイント設定
        public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

        // 座標追従無効
        public bool DisablePosition { get; set; }
        // 回転追従無効
        public bool DisableRotation { get; set; }
        // 拡縮追従無効
        public bool DisableLocalScale { get; set; }

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
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            var space = Settings.space;

            if (!DisablePosition) {
                var offsetPosition = Settings.offsetPosition;

                if (space == Space.Self) {
                    offsetPosition = Owner.TransformVector(offsetPosition);
                }

                Owner.position = GetTargetPosition() + offsetPosition;
            }

            if (!DisableRotation) {
                var offsetRotation = Quaternion.Euler(Settings.offsetAngles);

                if (space == Space.Self) {
                    Owner.rotation = GetTargetRotation() * offsetRotation;
                }
                else {
                    Owner.rotation = offsetRotation * GetTargetRotation();
                }
            }

            if (!DisableLocalScale) {
                var offsetScale = Settings.offsetScale;
                Owner.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
            }
        }

        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ParentConstraintJobHandle IJobParentConstraint.CreateJobHandle(Animator animator) {
            var handle = new ParentConstraintJobHandle();
            handle.space = Settings.space;
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