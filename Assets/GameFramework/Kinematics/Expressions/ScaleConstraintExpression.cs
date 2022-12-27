using System;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡縮コンストレイント
    /// </summary>
    public class ScaleConstraintExpression : ConstraintExpression, IJobScaleConstraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Vector3 offsetScale = Vector3.one;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            get => _settings;
            set => _settings = value;
        }

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        ScaleConstraintJobHandle IJobScaleConstraint.CreateJobHandle(Animator animator) {
            var handle = new ScaleConstraintJobHandle();
            handle.offsetScale = Settings.offsetScale;
            handle.ownerHandle = animator.BindStreamTransform(transform);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}