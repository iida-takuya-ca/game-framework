using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢コンストレイント
    /// </summary>
    public class RotationConstraintExpression : ConstraintExpression, IJobRotationConstraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
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
        RotationConstraintJobHandle IJobRotationConstraint.CreateJobHandle(Animator animator) {
            var handle = new RotationConstraintJobHandle();
            handle.space = Settings.space;
            handle.offsetRotation = quaternion.Euler(Settings.offsetAngles);
            handle.ownerHandle = animator.BindStreamTransform(transform);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}