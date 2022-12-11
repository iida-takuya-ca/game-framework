﻿using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡大縮小コンストレイント
    /// </summary>
    public class ScaleRuntimeConstraint : RuntimeConstraint, IJobScaleConstraint {
        // コンストレイント設定
        public class ConstraintSettings {
            public Vector3 offsetScale = Vector3.one;
        }

        // コンストレイント設定
        public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScaleRuntimeConstraint(Transform owner, string targetName = "")
            : base(owner, targetName) {
        }

        public ScaleRuntimeConstraint(Transform owner, Transform target)
            : base(owner, target) {
        }

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        ScaleConstraintJobHandle IJobScaleConstraint.CreateJobHandle(Animator animator) {
            var handle = new ScaleConstraintJobHandle();
            handle.offsetScale = Settings.offsetScale;
            handle.ownerHandle = animator.BindStreamTransform(Owner);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected override void ApplyTransform() {
            Owner.localScale = Vector3.Scale(GetTargetLocalScale(), Settings.offsetScale);
        }
    }
}