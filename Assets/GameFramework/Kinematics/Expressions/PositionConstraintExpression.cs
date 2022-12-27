using System;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標コンストレイント
    /// </summary>
    public class PositionConstraintExpression : ConstraintExpression, IJobPositionConstraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition;
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
        PositionConstraintJobHandle IJobPositionConstraint.CreateJobHandle(Animator animator) {
            var handle = new PositionConstraintJobHandle();
            handle.space = Settings.space;
            handle.offsetPosition = Settings.offsetPosition;
            handle.ownerHandle = animator.BindStreamTransform(transform);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}